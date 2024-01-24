using Kentico.Xperience.CRM.Common.Classes;

namespace Kentico.Xperience.CRM.Common.Services;

/// <summary>
/// Service for getting admin CRM settings
/// </summary>
public interface ICRMSettingsService
{
    CRMIntegrationSettingsInfo? GetSettings(string crmName);
}