using CMS.ContactManagement;

namespace Kentico.Xperience.CRM.Common.Services;

public interface IContactsIntegrationService
{
    /// <summary>
    /// Creates lead in CRM from Contact info
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <returns></returns>
    Task SynchronizeContactToLeadsAsync(ContactInfo contactInfo);

    /// <summary>
    /// Creates contact in CRM from Contact info
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <returns></returns>
    Task SynchronizeContactToContactsAsync(ContactInfo contactInfo);

    /// <summary>
    /// Creates or updates contact info from CRM lead
    /// </summary>
    /// <returns></returns>
    Task SynchronizeLeadsToKenticoAsync(DateTime lastSync);

    /// <summary>
    /// Creates or updates contact info from CRM contact
    /// </summary>
    /// <returns></returns>
    Task SynchronizeContactsToKenticoAsync(DateTime lastSync);
}