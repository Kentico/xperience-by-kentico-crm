using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Enums;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common setting for Kentico-CRM integration
/// </summary>
/// <typeparam name="TApiConfig"></typeparam>
public class CommonIntegrationSettings<TApiConfig> where TApiConfig : new()
{
    /// <summary>
    /// If enabled form submissions for registered forms are sent to CRM Leads
    /// </summary>
    public bool FormLeadsEnabled { get; set; }

    /// <summary>
    /// If enabled online marketing contacts are synced to CRM Leads or Contacts
    /// </summary>
    public bool ContactsEnabled { get; set; }

    public bool ContactsTwoWaySyncEnabled { get; set; }

    /// <summary>
    /// Where to sync contact - Leads/Contacts
    /// </summary>
    public ContactCRMType ContactType { get; set; }

    /// <summary>
    /// If true no existing item with same email or paired record by ID is updated
    /// </summary>
    public bool IgnoreExistingRecords { get; set; }

    /// <summary>
    /// Specific CRM API configuration
    /// </summary>
    public TApiConfig ApiConfig { get; set; } = new();
}