using Kentico.Xperience.CRM.Common.Mapping;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// One visually configured mapping: the CRM field being filled and the expression producing its value.
/// Mappings are target oriented - a CRM field appears at most once, and may draw on several contact fields.
/// </summary>
public class ContactFieldMappingDefinition
{
    /// <summary>
    /// Name of the CRM field the value is written to.
    /// </summary>
    public string CRMField { get; set; } = string.Empty;

    /// <summary>
    /// Expression producing the value.
    /// </summary>
    public ContactFieldValueExpression Expression { get; set; } = new();

    /// <summary>
    /// Order of the mapping in the admin UI and during synchronization.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// When <see langword="false"/>, the mapping is kept but not applied during synchronization.
    /// </summary>
    public bool Enabled { get; set; } = true;
}