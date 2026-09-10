namespace Kentico.Xperience.CRM.Common.Metadata;

/// <summary>
/// Describes a single Xperience contact field which can be used as a mapping source.
/// </summary>
public class ContactFieldMetadata
{
    /// <summary>
    /// Contact field name, as used in <c>{{Token}}</c> templates and stored in the mapping configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Label shown to marketers. Falls back to the field name when the field has no caption.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Xperience field data type, for example <c>text</c> or <c>integer</c>.
    /// </summary>
    public string DataType { get; set; } = string.Empty;
}