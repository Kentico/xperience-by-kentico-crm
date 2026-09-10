using System.Globalization;

using CMS.ContactManagement;
using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;
using Kentico.Xperience.CRM.Common.Metadata;

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Base page letting marketers map Xperience contact fields onto the fields of a CRM entity without code.
/// Mappings are target oriented - each row owns one CRM field and describes how its value is produced,
/// which is what allows several contact fields to feed a single CRM field.
/// A stored configuration fully replaces the mappings registered in code for that CRM and entity type.
/// </summary>
public abstract class ContactFieldMappingPage : Page<ContactFieldMappingClientProperties>
{
    private readonly IContactFieldMappingService mappingService;
    private readonly IContactFieldSourceProvider sourceProvider;
    private readonly IEnumerable<IContactSourceValueResolver> resolvers;
    private readonly IInfoProvider<ContactInfo> contactInfoProvider;

    protected ContactFieldMappingPage(IContactFieldMappingService mappingService,
        IContactFieldSourceProvider sourceProvider,
        IEnumerable<IContactSourceValueResolver> resolvers,
        IInfoProvider<ContactInfo> contactInfoProvider)
    {
        this.mappingService = mappingService;
        this.sourceProvider = sourceProvider;
        this.resolvers = resolvers;
        this.contactInfoProvider = contactInfoProvider;
    }

    /// <summary>
    /// CRM the page configures. See <see cref="CRMType"/>.
    /// </summary>
    protected abstract string CRMName { get; }

    /// <summary>
    /// Entity type the integration is registered to synchronize into. Used as the initially selected target.
    /// </summary>
    protected abstract ContactCRMType ConfiguredContactType { get; }

    /// <summary>
    /// Supplies the CRM field metadata.
    /// </summary>
    protected abstract ICRMEntityMetadataProvider MetadataProvider { get; }

    /// <summary>
    /// Returns the mapping the integration applies out of the box for the given entity type, offered in the
    /// UI as a starting point.
    /// </summary>
    /// <param name="entityType">Entity type. See <see cref="EntityType"/>.</param>
    protected abstract IReadOnlyList<ContactFieldMappingDefinition> GetDefaultSuggestions(string entityType);

    public override async Task<ContactFieldMappingClientProperties> ConfigureTemplateProperties(
        ContactFieldMappingClientProperties properties)
    {
        properties.CrmName = CRMName;
        properties.EntityTypes = new List<SelectOptionModel>
        {
            new() { Value = EntityType.Lead, Label = "Lead" },
            new() { Value = EntityType.Contact, Label = "Contact" }
        };
        properties.SourceFields = GetSourceFields();
        properties.Resolvers = GetResolvers();

        var data = await GetEntityTypeData(ToEntityType(ConfiguredContactType));

        properties.EntityType = data.EntityType;
        properties.CrmFields = data.CrmFields;
        properties.Mappings = data.Mappings;
        properties.MetadataIsLive = data.MetadataIsLive;
        properties.MetadataWarning = data.MetadataWarning;
        properties.HasConfiguration = data.HasConfiguration;

        return properties;
    }

    /// <summary>
    /// Loads the CRM fields and stored mappings of another entity type without leaving the page.
    /// </summary>
    [PageCommand]
    public async Task<ICommandResponse<ContactFieldMappingDataModel>> ChangeEntityType(
        ChangeEntityTypeArguments arguments)
    {
        var data = await GetEntityTypeData(NormalizeEntityType(arguments?.EntityType));

        return ResponseFrom(data);
    }

    /// <summary>
    /// Replaces the stored mapping of the given entity type. An empty set removes the configuration and
    /// returns the integration to the mappings defined in code.
    /// </summary>
    [PageCommand]
    public async Task<ICommandResponse<ContactFieldMappingDataModel>> Save(SaveMappingsArguments arguments)
    {
        string entityType = NormalizeEntityType(arguments?.EntityType);
        var rows = arguments?.Mappings ?? new List<ContactFieldMappingRowModel>();

        var metadata = await MetadataProvider.GetEntityMetadataAsync(entityType);
        var errors = ValidateRows(rows, metadata);

        if (errors.Count > 0)
        {
            var unchanged = await GetEntityTypeData(entityType);

            return errors.Aggregate(ResponseFrom(unchanged), (response, error) => response.AddErrorMessage(error));
        }

        mappingService.SaveDefinitions(CRMName, entityType, rows.Select(ToDefinition));

        var result = ResponseFrom(await GetEntityTypeData(entityType));

        result = rows.Count == 0
            ? result.AddSuccessMessage("Mapping configuration removed. The mapping defined in code is used again.")
            : result.AddSuccessMessage($"Saved {rows.Count} field mapping(s).");

        foreach (string warning in GetWarnings(rows, metadata))
        {
            result = result.AddWarningMessage(warning);
        }

        return result;
    }

    /// <summary>
    /// Fills the grid with the mapping the integration applies out of the box. The suggestion is only
    /// returned to the client - nothing is stored until the marketer saves.
    /// </summary>
    [PageCommand]
    public Task<ICommandResponse<LoadDefaultsResultModel>> LoadDefaults(ChangeEntityTypeArguments arguments)
    {
        var rows = GetDefaultSuggestions(NormalizeEntityType(arguments?.EntityType)).Select(ToRow).ToList();

        return Task.FromResult(ResponseFrom(new LoadDefaultsResultModel { Mappings = rows })
            .AddInfoMessage($"Loaded {rows.Count} default mapping(s). Review them and select Save to apply."));
    }

    /// <summary>
    /// Evaluates a single row against a real contact so the marketer can see what would be sent.
    /// </summary>
    [PageCommand]
    public Task<ICommandResponse<PreviewResultModel>> Preview(PreviewArguments arguments)
    {
        var row = arguments?.Mapping;

        if (row is null)
        {
            return Task.FromResult(ResponseFrom(new PreviewResultModel { Error = "Nothing to preview." }));
        }

        var sampleContact = GetSampleContact();

        if (sampleContact is null)
        {
            return Task.FromResult(ResponseFrom(new PreviewResultModel
            {
                Error = "No contact with an email address exists yet, so the value cannot be previewed."
            }));
        }

        try
        {
            object value = mappingService.Evaluate(ToDefinition(row).Expression, sampleContact);

            return Task.FromResult(ResponseFrom(new PreviewResultModel
            {
                Value = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
                SampleContact = sampleContact.ContactEmail
            }));
        }
        catch (Exception e)
        {
            // A preview must never take the page down - a bad expression is reported inline instead.
            return Task.FromResult(ResponseFrom(new PreviewResultModel { Error = e.Message }));
        }
    }

    private async Task<ContactFieldMappingDataModel> GetEntityTypeData(string entityType)
    {
        var metadata = await MetadataProvider.GetEntityMetadataAsync(entityType);

        return new ContactFieldMappingDataModel
        {
            EntityType = entityType,
            CrmFields = metadata.Fields.Select(ToTargetField).ToList(),
            Mappings = mappingService.GetDefinitions(CRMName, entityType).Select(ToRow).ToList(),
            MetadataIsLive = metadata.IsLive,
            MetadataWarning = metadata.Warning,
            HasConfiguration = mappingService.HasConfiguration(CRMName, entityType)
        };
    }

    private List<ContactSourceFieldModel> GetSourceFields() => sourceProvider.GetContactFields()
        .Select(f => new ContactSourceFieldModel
        {
            Name = f.Name,
            DisplayName = f.DisplayName,
            DataType = f.DataType
        })
        .ToList();

    private List<ResolverModel> GetResolvers() => resolvers
        .Select(r => new ResolverModel
        {
            Name = r.Name,
            DisplayName = r.DisplayName,
            DefaultSourceField = r.DefaultSourceField
        })
        .OrderBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)
        .ToList();

    private ContactInfo? GetSampleContact() => contactInfoProvider.Get()
        .WhereNotEmpty(nameof(ContactInfo.ContactEmail))
        .OrderByDescending(nameof(ContactInfo.ContactLastModified))
        .TopN(1)
        .FirstOrDefault();

    /// <summary>
    /// Rejects a configuration which cannot work, so that a broken mapping is never stored.
    /// </summary>
    private static List<string> ValidateRows(List<ContactFieldMappingRowModel> rows, CRMEntityMetadata metadata)
    {
        var errors = new List<string>();

        var duplicates = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.CrmField))
            .GroupBy(r => r.CrmField, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (string duplicate in duplicates)
        {
            errors.Add(
                $"The CRM field {duplicate} is mapped more than once. Each CRM field can be mapped only once.");
        }

        if (rows.Exists(r => string.IsNullOrWhiteSpace(r.CrmField)))
        {
            errors.Add("Every mapping must have a CRM field selected.");
        }

        foreach (var row in rows.Where(r => !string.IsNullOrWhiteSpace(r.CrmField)))
        {
            string? error = ValidateExpression(row);

            if (error is not null)
            {
                errors.Add($"{row.CrmField}: {error}");
            }
        }

        // Unknown target fields are only an error when the real CRM schema is available - the compiled
        // fallback does not contain customer specific fields, and rejecting them would be wrong.
        if (metadata.IsLive)
        {
            var known = metadata.Fields.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows.Where(r =>
                         !string.IsNullOrWhiteSpace(r.CrmField) && !known.Contains(r.CrmField)))
            {
                errors.Add(
                    $"The CRM field {row.CrmField} does not exist in {metadata.EntityType} or is not writable.");
            }
        }

        return errors;
    }

    private static string? ValidateExpression(ContactFieldMappingRowModel row) => ParseKind(row.Kind) switch
    {
        ContactFieldValueKind.SourceField when string.IsNullOrWhiteSpace(row.SourceField) =>
            "select a contact field.",
        ContactFieldValueKind.Template when string.IsNullOrWhiteSpace(row.Template) =>
            "the template is empty.",
        ContactFieldValueKind.Constant when string.IsNullOrWhiteSpace(row.ConstantValue) =>
            "the constant value is empty.",
        ContactFieldValueKind.Coalesce when row.SourceFields.Count == 0 =>
            "add at least one contact field to fall back through.",
        ContactFieldValueKind.Resolver when string.IsNullOrWhiteSpace(row.Resolver) =>
            "select a resolver.",
        _ => null
    };

    /// <summary>
    /// Reports conditions worth flagging which are not reasons to refuse the configuration.
    /// </summary>
    private static IEnumerable<string> GetWarnings(List<ContactFieldMappingRowModel> rows,
        CRMEntityMetadata metadata)
    {
        if (rows.Count == 0)
        {
            yield break;
        }

        var mapped = rows.Where(r => r.Enabled).Select(r => r.CrmField).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unmappedRequired = metadata.Fields
            .Where(f => f.IsRequired && !mapped.Contains(f.Name))
            .Select(f => f.DisplayName)
            .ToList();

        if (unmappedRequired.Count > 0)
        {
            yield return "These required " + metadata.EntityType +
                         " fields are not mapped and may cause the CRM to reject records: " +
                         string.Join(", ", unmappedRequired) + ".";
        }

        if (!metadata.IsLive)
        {
            yield return
                "The CRM schema could not be retrieved, so the CRM field names were not verified against your CRM.";
        }
    }

    private static CRMTargetFieldModel ToTargetField(CRMFieldMetadata field) => new()
    {
        Name = field.Name,
        DisplayName = field.DisplayName,
        DataType = field.DataType,
        IsRequired = field.IsRequired,
        IsCustom = field.IsCustom,
        MaxLength = field.MaxLength,
        Options = field.Options
            .Select(o => new SelectOptionModel { Value = o.Value, Label = o.Label })
            .ToList()
    };

    private static ContactFieldMappingRowModel ToRow(ContactFieldMappingDefinition definition) => new()
    {
        CrmField = definition.CRMField,
        Kind = definition.Expression.Kind.ToString(),
        SourceField = definition.Expression.SourceField,
        Template = definition.Expression.Template,
        ConstantValue = definition.Expression.ConstantValue,
        SourceFields = definition.Expression.SourceFields.ToList(),
        Resolver = definition.Expression.Resolver,
        Enabled = definition.Enabled
    };

    private static ContactFieldMappingDefinition ToDefinition(ContactFieldMappingRowModel row) => new()
    {
        CRMField = row.CrmField,
        Enabled = row.Enabled,
        Expression = new ContactFieldValueExpression
        {
            Kind = ParseKind(row.Kind),
            SourceField = row.SourceField,
            Template = row.Template,
            ConstantValue = row.ConstantValue,
            SourceFields = row.SourceFields.Where(f => !string.IsNullOrWhiteSpace(f)).ToList(),
            Resolver = row.Resolver
        }
    };

    private static ContactFieldValueKind ParseKind(string? kind) =>
        Enum.TryParse<ContactFieldValueKind>(kind, true, out var parsed)
            ? parsed
            : ContactFieldValueKind.SourceField;

    private static string ToEntityType(ContactCRMType contactType) =>
        contactType == ContactCRMType.Contact ? EntityType.Contact : EntityType.Lead;

    private static string NormalizeEntityType(string? entityType) =>
        string.Equals(entityType, EntityType.Contact, StringComparison.OrdinalIgnoreCase)
            ? EntityType.Contact
            : EntityType.Lead;
}