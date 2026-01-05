using Kentico.Xperience.CRM.Common.Mapping;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common BizForms to CRM Leads mapping configuration
/// </summary>
public class BizFormsMappingConfiguration
{
    public Dictionary<string, List<BizFormFieldMapping>> FormsMappings { get; init; } = new();
    public Dictionary<string, List<Type>> FormsConverters { get; init; } = new();
}