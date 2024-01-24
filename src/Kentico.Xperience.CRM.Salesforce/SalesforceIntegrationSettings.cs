using Kentico.Xperience.CRM.Common.Configuration;

namespace Kentico.Xperience.CRM.Salesforce.Configuration;

/// <summary>
/// Specific setting for Salesforce Sales integration 
/// </summary>
public class SalesforceIntegrationSettings : CommonIntegrationSettings<SalesforceApiConfig>
{
    public const string ConfigKeyName = "CMSSalesforceCRMIntegration";
}