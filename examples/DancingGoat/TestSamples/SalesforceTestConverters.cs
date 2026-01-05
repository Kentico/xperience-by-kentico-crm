using CMS.ContactManagement;

using Kentico.Xperience.CRM.Common.Converters;

using Salesforce.OpenApi;

namespace DancingGoat.TestSamples;

internal class SalesforceContactToLeadCustomConverter : ICRMTypeConverter<ContactInfo, LeadSObject>
{
    public Task Convert(ContactInfo source, LeadSObject destination)
    {
        //some mapping when updating
        if (destination.Id != null)
        {
            destination.Email = source.ContactEmail;
        }
        //mapping for create
        else
        {
            destination.Email = source.ContactEmail;
        }

        return Task.CompletedTask;
    }
}

internal class SalesforceContactToContactCustomConverter : ICRMTypeConverter<ContactInfo, ContactSObject>
{
    public Task Convert(ContactInfo source, ContactSObject destination)
    {
        //some mapping when updating
        if (destination.Id != null)
        {
            destination.Email = source.ContactEmail;
        }
        //mapping for create
        else
        {
            destination.Email = source.ContactEmail;
        }

        return Task.CompletedTask;
    }
}

internal class SalesforceLeadToKenticoContactCustomConverter : ICRMTypeConverter<LeadSObject, ContactInfo>
{
    public Task Convert(LeadSObject source, ContactInfo destination)
    {
        if (destination.ContactID == 0)
        {
            // mapping on create
            destination.ContactEmail = source.Email;
            destination.ContactFirstName = source.FirstName;
            destination.ContactLastName = source.LastName;
        }
        else
        {
            // mapping on update
            destination.ContactNotes = $"Status: {source.Status}";
        }

        return Task.CompletedTask;
    }
}

internal class SalesforceContactToKenticoContactCustomConverter : ICRMTypeConverter<ContactSObject, ContactInfo>
{
    public Task Convert(ContactSObject source, ContactInfo destination)
    {
        if (destination.ContactID == 0)
        {
            // mapping on create
            destination.ContactEmail = source.Email;
            destination.ContactFirstName = source.FirstName;
            destination.ContactLastName = source.LastName;
        }
        else
        {
            // mapping on update
            destination.ContactNotes = $"Status: {source.CleanStatus}";
        }

        return Task.CompletedTask;
    }
}