using System.Reflection;

using CMS.Helpers;

using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Metadata;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Kentico.Xperience.CRM.Dynamics.Metadata;

/// <summary>
/// Retrieves the attribute metadata of a Dataverse entity, so the field mapping UI offers the fields that
/// actually exist in the customer's environment, including custom ones. When Dataverse cannot be reached
/// the attributes of the compiled early bound entity classes are used instead.
/// </summary>
internal class DynamicsEntityMetadataProvider : ICRMEntityMetadataProvider
{
    /// <summary>
    /// Metadata changes rarely and a retrieval is expensive, so it is cached. Failures are cached as well,
    /// which keeps an unreachable or unauthorized environment from being called on every request.
    /// </summary>
    private const int CacheMinutes = 10;

    private readonly IServiceProvider serviceProvider;
    private readonly IProgressiveCache cache;
    private readonly ILogger<DynamicsEntityMetadataProvider> logger;

    public DynamicsEntityMetadataProvider(IServiceProvider serviceProvider,
        IProgressiveCache cache,
        ILogger<DynamicsEntityMetadataProvider> logger)
    {
        this.serviceProvider = serviceProvider;
        this.cache = cache;
        this.logger = logger;
    }

    public string CRMName => CRMType.Dynamics;

    public Task<CRMEntityMetadata> GetEntityMetadataAsync(string entityType,
        CancellationToken cancellationToken = default) =>
        cache.LoadAsync(_ => RetrieveMetadataAsync(entityType, cancellationToken),
            new CacheSettings(CacheMinutes, $"{nameof(DynamicsEntityMetadataProvider)}|{entityType}"));

    private async Task<CRMEntityMetadata> RetrieveMetadataAsync(string entityType,
        CancellationToken cancellationToken)
    {
        try
        {
            // Resolved lazily: the client throws when the integration has no API settings yet, which is
            // exactly the situation in which the fallback metadata has to be used.
            var serviceClient = serviceProvider.GetRequiredService<ServiceClient>();

            var request = new RetrieveEntityRequest
            {
                LogicalName = entityType,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request, cancellationToken);

            var fields = response.EntityMetadata.Attributes
                .Where(IsMappable)
                .Select(ToFieldMetadata)
                .OrderBy(f => f.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new CRMEntityMetadata { EntityType = entityType, IsLive = true, Fields = fields };
        }
        catch (Exception e)
        {
            logger.LogWarning(e,
                "Dataverse metadata for {EntityType} could not be retrieved, using the compiled entity definition",
                entityType);

            return GetFallbackMetadata(entityType, e.Message);
        }
    }

    /// <summary>
    /// Keeps the attributes a value can actually be written to. Attributes which only exist to back
    /// another one, such as the formatted value of a lookup, are excluded.
    /// </summary>
    private static bool IsMappable(AttributeMetadata attribute) =>
        attribute.AttributeOf is null &&
        attribute.LogicalName is not null &&
        ((attribute.IsValidForCreate ?? false) || (attribute.IsValidForUpdate ?? false)) &&
        attribute.AttributeType != AttributeTypeCode.Virtual;

    private static CRMFieldMetadata ToFieldMetadata(AttributeMetadata attribute) => new()
    {
        Name = attribute.LogicalName,
        DisplayName = attribute.DisplayName?.UserLocalizedLabel?.Label is { Length: > 0 } label
            ? $"{label} ({attribute.LogicalName})"
            : attribute.LogicalName,
        DataType = ToDataType(attribute.AttributeType),
        IsRequired = attribute.RequiredLevel?.Value is AttributeRequiredLevel.ApplicationRequired
            or AttributeRequiredLevel.SystemRequired,
        IsCustom = attribute.IsCustomAttribute ?? false,
        MaxLength = (attribute as StringAttributeMetadata)?.MaxLength ??
                    (attribute as MemoAttributeMetadata)?.MaxLength,
        NativeType = attribute.AttributeType?.ToString() ?? string.Empty,
        ReferenceTargets = (attribute as LookupAttributeMetadata)?.Targets?.ToList() ?? new List<string>(),
        Options = GetOptions(attribute)
    };

    private static List<CRMFieldOption> GetOptions(AttributeMetadata attribute)
    {
        var optionSet = attribute switch
        {
            EnumAttributeMetadata enumAttribute => enumAttribute.OptionSet,
            _ => null
        };

        if (optionSet?.Options is null)
        {
            return new List<CRMFieldOption>();
        }

        return optionSet.Options
            .Where(o => o.Value.HasValue)
            .Select(o => new CRMFieldOption
            {
                Value = o.Value.GetValueOrDefault().ToString(),
                Label = o.Label?.UserLocalizedLabel?.Label ?? o.Value.GetValueOrDefault().ToString()
            })
            .ToList();
    }

    private static string ToDataType(AttributeTypeCode? attributeType) => attributeType switch
    {
        AttributeTypeCode.String or AttributeTypeCode.Memo => CRMFieldDataTypes.String,
        AttributeTypeCode.Integer or AttributeTypeCode.BigInt => CRMFieldDataTypes.Integer,
        AttributeTypeCode.Decimal or AttributeTypeCode.Double => CRMFieldDataTypes.Decimal,
        AttributeTypeCode.Money => CRMFieldDataTypes.Decimal,
        AttributeTypeCode.Boolean => CRMFieldDataTypes.Boolean,
        AttributeTypeCode.DateTime => CRMFieldDataTypes.DateTime,
        AttributeTypeCode.Uniqueidentifier => CRMFieldDataTypes.Guid,
        AttributeTypeCode.Picklist or AttributeTypeCode.State or AttributeTypeCode.Status =>
            CRMFieldDataTypes.Picklist,
        AttributeTypeCode.Lookup or AttributeTypeCode.Customer or AttributeTypeCode.Owner =>
            CRMFieldDataTypes.Reference,
        _ => CRMFieldDataTypes.String
    };

    /// <summary>
    /// Builds the field list from the compiled early bound entity classes. Custom attributes of the
    /// customer's environment are not included, which is why the UI reports this metadata as not live.
    /// </summary>
    private static CRMEntityMetadata GetFallbackMetadata(string entityType, string reason)
    {
        var entityClrType = string.Equals(entityType, EntityType.Contact, StringComparison.OrdinalIgnoreCase)
            ? typeof(Contact)
            : typeof(Lead);

        var fields = entityClrType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new
            {
                Property = p,
                LogicalName = p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName
            })
            .Where(p => !string.IsNullOrEmpty(p.LogicalName) && p.Property.CanWrite)
            .GroupBy(p => p.LogicalName!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CRMFieldMetadata
            {
                Name = g.Key,
                DisplayName = $"{g.First().Property.Name} ({g.Key})",
                DataType = ToDataTypeFromClrType(g.First().Property.PropertyType),
                NativeType = ToNativeType(g.First().Property.PropertyType)
            })
            .OrderBy(f => f.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new CRMEntityMetadata
        {
            EntityType = entityType,
            IsLive = false,
            Warning = reason,
            Fields = fields
        };
    }

    private static string ToDataTypeFromClrType(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(string))
        {
            return CRMFieldDataTypes.String;
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return CRMFieldDataTypes.Integer;
        }

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(Money))
        {
            return CRMFieldDataTypes.Decimal;
        }

        if (type == typeof(bool))
        {
            return CRMFieldDataTypes.Boolean;
        }

        if (type == typeof(DateTime))
        {
            return CRMFieldDataTypes.DateTime;
        }

        if (type == typeof(Guid))
        {
            return CRMFieldDataTypes.Guid;
        }

        if (type == typeof(OptionSetValue))
        {
            return CRMFieldDataTypes.Picklist;
        }

        if (type == typeof(EntityReference))
        {
            return CRMFieldDataTypes.Reference;
        }

        return CRMFieldDataTypes.String;
    }

    /// <summary>
    /// Derives the Dataverse attribute type name from the property type of the compiled entity class,
    /// so that values written against fallback metadata are still converted to the right CLR type.
    /// </summary>
    private static string ToNativeType(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(string))
        {
            return nameof(AttributeTypeCode.String);
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return nameof(AttributeTypeCode.Integer);
        }

        if (type == typeof(Money))
        {
            return nameof(AttributeTypeCode.Money);
        }

        if (type == typeof(decimal))
        {
            return nameof(AttributeTypeCode.Decimal);
        }

        if (type == typeof(double))
        {
            return nameof(AttributeTypeCode.Double);
        }

        if (type == typeof(bool))
        {
            return nameof(AttributeTypeCode.Boolean);
        }

        if (type == typeof(DateTime))
        {
            return nameof(AttributeTypeCode.DateTime);
        }

        if (type == typeof(Guid))
        {
            return nameof(AttributeTypeCode.Uniqueidentifier);
        }

        if (type == typeof(OptionSetValue))
        {
            return nameof(AttributeTypeCode.Picklist);
        }

        if (type == typeof(EntityReference))
        {
            return nameof(AttributeTypeCode.Lookup);
        }

        return nameof(AttributeTypeCode.String);
    }
}