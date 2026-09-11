using System.Reflection;
using System.Text.Json.Serialization;

using CMS.Helpers;

using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Metadata;
using Kentico.Xperience.CRM.Salesforce.Synchronization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Metadata;

/// <summary>
/// Retrieves the field metadata of a Salesforce sObject through the describe resource, so the field mapping
/// UI offers the fields of the customer's org, including custom ones. When Salesforce cannot be reached the
/// properties of the generated sObject classes are used instead.
/// </summary>
internal class SalesforceEntityMetadataProvider : ICRMEntityMetadataProvider
{
    /// <summary>
    /// A describe response is large and changes rarely, so it is cached. Failures are cached as well, which
    /// keeps an unreachable or unauthorized org from being called on every request.
    /// </summary>
    private const int CacheMinutes = 10;

    private readonly IServiceProvider serviceProvider;
    private readonly IProgressiveCache cache;
    private readonly ILogger<SalesforceEntityMetadataProvider> logger;

    public SalesforceEntityMetadataProvider(IServiceProvider serviceProvider,
        IProgressiveCache cache,
        ILogger<SalesforceEntityMetadataProvider> logger)
    {
        this.serviceProvider = serviceProvider;
        this.cache = cache;
        this.logger = logger;
    }

    public string CRMName => CRMType.Salesforce;

    public Task<CRMEntityMetadata> GetEntityMetadataAsync(string entityType,
        CancellationToken cancellationToken = default) =>
        cache.LoadAsync(_ => RetrieveMetadataAsync(entityType, cancellationToken),
            new CacheSettings(CacheMinutes, $"{nameof(SalesforceEntityMetadataProvider)}|{entityType}"));

    private async Task<CRMEntityMetadata> RetrieveMetadataAsync(string entityType,
        CancellationToken cancellationToken)
    {
        bool isContact = IsContact(entityType);

        try
        {
            // Resolved lazily: the API client throws when the integration has no API settings yet, which
            // is exactly the situation in which the fallback metadata has to be used.
            var apiService = serviceProvider.GetRequiredService<ISalesforceApiService>();

            var describe = await apiService.DescribeSObjectAsync(isContact ? "Contact" : "Lead",
                cancellationToken);

            if (describe is null)
            {
                return GetFallbackMetadata(entityType, "Salesforce returned no field metadata");
            }

            var fields = describe.Fields
                .Where(IsMappable)
                .Select(ToFieldMetadata)
                .OrderBy(f => f.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new CRMEntityMetadata { EntityType = entityType, IsLive = true, Fields = fields };
        }
        catch (Exception e)
        {
            logger.LogWarning(e,
                "Salesforce metadata for {EntityType} could not be retrieved, using the generated sObject definition",
                entityType);

            return GetFallbackMetadata(entityType, e.Message);
        }
    }

    private static bool IsContact(string entityType) =>
        string.Equals(entityType, EntityType.Contact, StringComparison.OrdinalIgnoreCase);

    private static bool IsMappable(SObjectFieldDescribe field) =>
        !string.IsNullOrEmpty(field.Name) && (field.Createable || field.Updateable);

    private static CRMFieldMetadata ToFieldMetadata(SObjectFieldDescribe field) => new()
    {
        Name = field.Name!,
        // The label alone - the admin UI has the field name in Name and shows it separately, so
        // repeating it here only produced duplicated text and truncated table cells.
        DisplayName = string.IsNullOrWhiteSpace(field.Label) ? field.Name! : field.Label!,
        DataType = ToDataType(field.Type),
        // Salesforce rejects a record without a value only when the field cannot be empty and Salesforce
        // does not fill it itself.
        IsRequired = !field.Nillable && field.Createable && !field.DefaultedOnCreate,
        IsCustom = field.Custom,
        MaxLength = field.Length > 0 ? field.Length : null,
        NativeType = field.Type ?? string.Empty,
        ReferenceTargets = field.ReferenceTo.ToList(),
        Options = field.PicklistValues
            .Where(v => v.Active && v.Value is not null)
            .Select(v => new CRMFieldOption
            {
                Value = v.Value!,
                Label = string.IsNullOrWhiteSpace(v.Label) ? v.Value! : v.Label!
            })
            .ToList()
    };

    private static string ToDataType(string? salesforceType) => salesforceType?.ToLowerInvariant() switch
    {
        "int" or "long" => CRMFieldDataTypes.Integer,
        "double" or "currency" or "percent" => CRMFieldDataTypes.Decimal,
        "boolean" => CRMFieldDataTypes.Boolean,
        "date" or "datetime" or "time" => CRMFieldDataTypes.DateTime,
        "picklist" or "multipicklist" or "combobox" => CRMFieldDataTypes.Picklist,
        "reference" => CRMFieldDataTypes.Reference,
        "id" => CRMFieldDataTypes.Guid,
        _ => CRMFieldDataTypes.String
    };

    /// <summary>
    /// Builds the field list from the generated sObject classes. Custom fields of the customer's org are
    /// not included, which is why the UI reports this metadata as not live.
    /// </summary>
    private static CRMEntityMetadata GetFallbackMetadata(string entityType, string reason)
    {
        var sObjectType = IsContact(entityType) ? typeof(ContactSObject) : typeof(LeadSObject);

        var fields = sObjectType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && !IsSObjectMetadata(p))
            .Select(p => new
            {
                Property = p,
                FieldName = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name
            })
            .GroupBy(p => p.FieldName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CRMFieldMetadata
            {
                Name = g.Key,
                DisplayName = g.Key,
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

    /// <summary>
    /// Excludes the sObject envelope, which describes the record rather than being a field of it.
    /// </summary>
    private static bool IsSObjectMetadata(PropertyInfo property) =>
        property.PropertyType.Name.Equals("Attributes", StringComparison.Ordinal);

    private static string ToDataTypeFromClrType(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(bool))
        {
            return CRMFieldDataTypes.Boolean;
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return CRMFieldDataTypes.Integer;
        }

        if (type == typeof(double) || type == typeof(decimal))
        {
            return CRMFieldDataTypes.Decimal;
        }

        if (type == typeof(DateTimeOffset) || type == typeof(DateTime))
        {
            return CRMFieldDataTypes.DateTime;
        }

        return CRMFieldDataTypes.String;
    }

    private static string ToNativeType(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return "int";
        }

        if (type == typeof(double) || type == typeof(decimal))
        {
            return "double";
        }

        if (type == typeof(DateTimeOffset) || type == typeof(DateTime))
        {
            return "datetime";
        }

        return "string";
    }
}