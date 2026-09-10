namespace Kentico.Xperience.CRM.Common.Metadata;

/// <summary>
/// Describes a single writable field of a CRM entity, as offered to marketers in the field mapping UI.
/// </summary>
public class CRMFieldMetadata
{
    /// <summary>
    /// Field name as used by the CRM API - a Dataverse attribute logical name or a Salesforce field name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Label shown to marketers.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Normalized data type. See <see cref="CRMFieldDataTypes"/>.
    /// </summary>
    public string DataType { get; set; } = CRMFieldDataTypes.String;

    /// <summary>
    /// Indicates that the CRM rejects the record when the field has no value.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates a field added to the CRM by the customer rather than a stock field.
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// Maximum text length, when the CRM reports one.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// CRM specific type name, kept alongside the normalized <see cref="DataType"/> because writing a
    /// value needs more fidelity than the UI does - a Dataverse money attribute and a decimal one are
    /// both decimals to a marketer but need different CLR types.
    /// </summary>
    public string NativeType { get; set; } = string.Empty;

    /// <summary>
    /// Entity types a reference field can point to. Used to build a reference value from an identifier.
    /// </summary>
    public List<string> ReferenceTargets { get; set; } = new();

    /// <summary>
    /// Allowed values for option set and picklist fields.
    /// </summary>
    public List<CRMFieldOption> Options { get; set; } = new();
}

/// <summary>
/// Single allowed value of an option set or picklist CRM field.
/// </summary>
public class CRMFieldOption
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Normalized CRM field data types. Both CRMs report their own type systems, which the metadata
/// providers reduce to this set so that the admin UI and the value converters stay CRM agnostic.
/// </summary>
public static class CRMFieldDataTypes
{
    public const string String = "string";
    public const string Integer = "integer";
    public const string Decimal = "decimal";
    public const string Boolean = "boolean";
    public const string DateTime = "datetime";
    public const string Guid = "guid";
    public const string Picklist = "picklist";
    public const string Reference = "reference";
}