namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Service for getting admin CRM settings
/// </summary>
public interface ICRMSettingsService
{
    CRMIntegrationSettingsInfo? GetSettings(string crmName);
}