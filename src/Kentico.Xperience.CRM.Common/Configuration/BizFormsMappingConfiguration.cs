using Kentico.Xperience.CRM.Common.Mapping.Implementations;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common BizForms to CRM Leads mapping configuration
/// </summary>
public class BizFormsMappingConfiguration
{
    public Dictionary<string, List<BizFormFieldMapping>> FormsMappings { get; internal init; } = new();
    public string? ExternalIdFieldName { get; internal init; }
}