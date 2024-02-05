using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Converters;
using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Configuration;

public class LeadToKenticoContactConverter : ICRMTypeConverter<LeadSObject, ContactInfo>
{
    public Task Convert(LeadSObject source, ContactInfo destination)
    {
        destination.ContactFirstName = source.FirstName;
        destination.ContactLastName = source.LastName;
        destination.ContactAddress1 = source.Street;
        destination.ContactCity = source.City;
        destination.ContactZIP = source.PostalCode;
        destination.ContactMobilePhone = source.MobilePhone;
        destination.ContactBusinessPhone = source.Phone;
        destination.ContactEmail = source.Email;
        destination.ContactNotes = source.Description;
        destination.ContactCompanyName = source.Company;

        return Task.CompletedTask;
    }
}