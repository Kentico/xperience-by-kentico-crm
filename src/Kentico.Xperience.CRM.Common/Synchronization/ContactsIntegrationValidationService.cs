using CMS.ContactManagement;
using CMS.Helpers;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

internal class ContactsIntegrationValidationService : IContactsIntegrationValidationService
{
    public Task<bool> ValidateContactInfo(ContactInfo contactInfo)
        => Task.FromResult(ValidationHelper.IsEmail(contactInfo.ContactEmail));
}