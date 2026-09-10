using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Properties passed to the <c>ContactFieldMapping</c> client template.
/// </summary>
public class ContactFieldMappingClientProperties : TemplateClientProperties
{
    /// <summary>
    /// CRM the page configures. See <see cref="Constants.CRMType"/>.
    /// </summary>
    public string CrmName { get; set; } = string.Empty;

    /// <summary>
    /// Currently edited CRM entity type. See <see cref="Constants.EntityType"/>.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity types the marketer can switch between.
    /// </summary>
    public List<SelectOptionModel> EntityTypes { get; set; } = new();

    /// <summary>
    /// Xperience contact fields available as mapping sources.
    /// </summary>
    public List<ContactSourceFieldModel> SourceFields { get; set; } = new();

    /// <summary>
    /// Named resolvers available for reference and coded contact fields.
    /// </summary>
    public List<ResolverModel> Resolvers { get; set; } = new();

    /// <summary>
    /// Writable fields of the target CRM entity.
    /// </summary>
    public List<CRMTargetFieldModel> CrmFields { get; set; } = new();

    /// <summary>
    /// Configured mappings, in order.
    /// </summary>
    public List<ContactFieldMappingRowModel> Mappings { get; set; } = new();

    /// <summary>
    /// <see langword="true"/> when <see cref="CrmFields"/> came from the CRM rather than from the
    /// compiled fallback.
    /// </summary>
    public bool MetadataIsLive { get; set; }

    /// <summary>
    /// Warning explaining why live metadata was not used.
    /// </summary>
    public string? MetadataWarning { get; set; }

    /// <summary>
    /// <see langword="true"/> when mappings are stored for this CRM and entity type, meaning the mappings
    /// defined in code are not applied.
    /// </summary>
    public bool HasConfiguration { get; set; }
}

/// <summary>
/// Generic value/label pair for the client selects.
/// </summary>
public class SelectOptionModel
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Xperience contact field offered as a mapping source.
/// </summary>
public class ContactSourceFieldModel
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;
}

/// <summary>
/// Named resolver offered as a mapping source.
/// </summary>
public class ResolverModel
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string DefaultSourceField { get; set; } = string.Empty;
}

/// <summary>
/// Writable CRM field offered as a mapping target.
/// </summary>
public class CRMTargetFieldModel
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public bool IsCustom { get; set; }

    public int? MaxLength { get; set; }

    public List<SelectOptionModel> Options { get; set; } = new();
}

/// <summary>
/// One row of the mapping grid. Mirrors <see cref="Mapping.ContactFieldValueExpression"/> flattened for
/// the client, so the template does not have to deal with a discriminated union.
/// </summary>
public class ContactFieldMappingRowModel
{
    /// <summary>
    /// Target CRM field name.
    /// </summary>
    public string CrmField { get; set; } = string.Empty;

    /// <summary>
    /// Value expression kind. See <see cref="Mapping.ContactFieldValueKind"/>.
    /// </summary>
    public string Kind { get; set; } = nameof(Mapping.ContactFieldValueKind.SourceField);

    public string? SourceField { get; set; }

    public string? Template { get; set; }

    public string? ConstantValue { get; set; }

    public List<string> SourceFields { get; set; } = new();

    public string? Resolver { get; set; }

    public bool Enabled { get; set; } = true;
}