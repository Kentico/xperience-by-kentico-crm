using CMS.ContactManagement;

using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

namespace DancingGoat.TestSamples;

internal class DynamicsContactToLeadCustomConverter : ICRMTypeConverter<ContactInfo, Lead>
{
    public Task Convert(ContactInfo source, Lead destination)
    {
        //some mapping when updating
        if (destination.Id != Guid.Empty)
        {
            destination.EMailAddress1 = source.ContactEmail;
        }
        //mapping for create
        else
        {
            destination.EMailAddress1 = source.ContactEmail;
        }

        return Task.CompletedTask;
    }
}

internal class DynamicsContactToContactCustomConverter : ICRMTypeConverter<ContactInfo, Contact>
{
    public Task Convert(ContactInfo source, Contact destination)
    {
        //some mapping when updating
        if (destination.Id != Guid.Empty)
        {
            destination.EMailAddress1 = source.ContactEmail;
        }
        //mapping for create
        else
        {
            destination.EMailAddress1 = source.ContactEmail;
        }

        return Task.CompletedTask;
    }
}

internal class DynamicsLeadToKenticoContactCustomConverter : ICRMTypeConverter<Lead, ContactInfo>
{
    public Task Convert(Lead source, ContactInfo destination)
    {
        if (destination.ContactID == 0)
        {
            // mapping on create
            destination.ContactEmail = source.EMailAddress1;
            destination.ContactFirstName = source.FirstName;
            destination.ContactLastName = source.LastName;
        }
        else
        {
            // mapping on update
            destination.ContactNotes = $"Status: {source.StatusCode?.ToString()}";
        }

        return Task.CompletedTask;
    }
}

internal class DynamicsContactToKenticoContactCustomConverter : ICRMTypeConverter<Contact, ContactInfo>
{
    public Task Convert(Contact source, ContactInfo destination)
    {
        if (destination.ContactID == 0)
        {
            // mapping on create
            destination.ContactEmail = source.EMailAddress1;
            destination.ContactFirstName = source.FirstName;
            destination.ContactLastName = source.LastName;
        }
        else
        {
            // mapping on update
            destination.ContactNotes = $"Status: {source.StatusCode?.ToString()}";
        }

        return Task.CompletedTask;
    }
}

