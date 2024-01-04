using Kentico.Xperience.CRM.Common.Configuration;

namespace Kentico.Xperience.CRM.SalesForce.Configuration;

/// <summary>
/// Specific setting for SalesForce Sales integration 
/// </summary>
public class SalesForceIntegrationSettings : CommonIntegrationSettings<SalesForceApiConfig>
{
    public const string ConfigKeyName = "CMSSalesForceCRMIntegration";
}