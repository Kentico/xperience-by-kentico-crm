using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Enums;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common setting for Kentico-CRM integration
/// </summary>
/// <typeparam name="TApiConfig"></typeparam>
public class CommonIntegrationSettings<TApiConfig>
{
    public bool FormLeadsEnabled { get; set; }
    public bool ContactsEnabled { get; set; }

    public ContactCRMType ContactType { get; set; }
    
    public TApiConfig? ApiConfig { get; set; }
}
