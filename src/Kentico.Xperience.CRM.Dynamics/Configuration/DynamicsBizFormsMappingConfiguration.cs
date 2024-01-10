using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

/// <summary>
/// Specific configuration for BizForm mapping to Lead in Dynamics Sales
/// </summary>
public class DynamicsBizFormsMappingConfiguration : BizFormsMappingConfiguration
{
    public Dictionary<string, List<Type>> FormsConverters { get; init; } = new();
}