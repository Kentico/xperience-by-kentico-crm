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
    /// 
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <returns></returns>
    Task SynchronizeContactToContactsAsync(ContactInfo contactInfo);
}