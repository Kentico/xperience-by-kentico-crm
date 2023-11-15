using Kentico.Xperience.CRM.Common.Configuration;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

/// <summary>
/// Specific setting for Dynamics Sales integration 
/// </summary>
public class DynamicsIntegrationSettings : CommonIntegrationSettings<DataverseApiConfig>
{
    public const string ConfigKeyName = "DynamicsCRMIntegration";
}
