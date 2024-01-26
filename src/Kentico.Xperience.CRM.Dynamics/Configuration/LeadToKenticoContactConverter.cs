using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

namespace Kentico.Xperience.CRM.Dynamics.Converters;

public class LeadToKenticoContactConverter : ICRMTypeConverter<Lead, ContactInfo>
{
     public Task Convert(Lead source, ContactInfo destination)
    {
        destination.ContactFirstName = source.FirstName;
        destination.ContactMiddleName = source.MiddleName;
        destination.ContactLastName = source.LastName;
        destination.ContactJobTitle = source.JobTitle;
        destination.ContactAddress1 = source.Address1_Line1;
        destination.ContactCity = source.Address1_City;
        destination.ContactZIP = source.Address1_PostalCode;
        destination.ContactMobilePhone = source.MobilePhone;
        destination.ContactBusinessPhone = source.Telephone1;
        destination.ContactEmail = source.EMailAddress1;
        destination.ContactNotes = source.Description;
        destination.ContactCompanyName = source.CompanyName;

        return Task.CompletedTask;
    }
}