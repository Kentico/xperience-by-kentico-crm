using CMS.ContactManagement;

namespace Kentico.Xperience.CRM.Common.Services;

public interface IContactsIntegrationService
{
    /// <summary>
    /// Creates lead in CRM from Contact info
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <returns></returns>
    Task CreateLeadAsync(ContactInfo contactInfo);
    
    /// <summary>
    /// Updated contact in CRM from Contact info
    /// </summary>
    /// <param name="bizFormItem"></param>
    /// <returns></returns>
    Task UpdateLeadAsync(ContactInfo contactInfo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <returns></returns>
    Task CreateContactAsync(ContactInfo contactInfo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <returns></returns>
    Task UpdateContactAsync(ContactInfo contactInfo);
}