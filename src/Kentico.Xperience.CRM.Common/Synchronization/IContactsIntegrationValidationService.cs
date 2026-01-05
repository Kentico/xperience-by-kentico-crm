using CMS.ContactManagement;

namespace Kentico.Xperience.CRM.Common.Services;

public interface IContactsIntegrationValidationService
{
    Task<bool> ValidateContactInfo(ContactInfo contactInfo);
}