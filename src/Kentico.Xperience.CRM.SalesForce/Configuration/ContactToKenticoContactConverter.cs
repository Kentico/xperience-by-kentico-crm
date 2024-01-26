using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Converters;
using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Configuration;

public class ContactToKenticoContactConverter : ICRMTypeConverter<ContactSObject, ContactInfo>
{
    public Task Convert(ContactSObject source, ContactInfo destination)
    {
        destination.ContactFirstName = source.FirstName;
        destination.ContactLastName = source.LastName;
        destination.ContactAddress1 = source.MailingStreet;
        destination.ContactCity = source.MailingCity;
        destination.ContactZIP = source.MailingPostalCode;
        destination.ContactMobilePhone = source.MobilePhone;
        destination.ContactBusinessPhone = source.Phone;
        destination.ContactEmail = source.Email;
        destination.ContactNotes = source.Description;

        return Task.CompletedTask;
    }
}