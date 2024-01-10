using CMS.ContactManagement;
using CMS.OnlineForms;
using CMS.OnlineForms.Internal;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

namespace Kentico.Xperience.CRM.Dynamics.Converters;

public class FormContactMappingToLeadConverter : ICRMTypeConverter<BizFormItem, Lead>
{
    private readonly IContactFieldFromFormRetriever contactFieldFromFormRetriever;

    public FormContactMappingToLeadConverter(IContactFieldFromFormRetriever contactFieldFromFormRetriever)
    {
        this.contactFieldFromFormRetriever = contactFieldFromFormRetriever;
    }

    public Task<Lead> Convert(BizFormItem source, Lead destination)
    {
        var firstName = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactFirstName));
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            destination.FirstName = firstName;
        }
        
        var lastName = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactLastName));
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            destination.LastName = lastName;
        }
        
        var email = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactEmail));
        if (!string.IsNullOrWhiteSpace(email))
        {
            destination.EMailAddress1 = email;
        }
        
        var companyName = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactCompanyName));
        if (!string.IsNullOrWhiteSpace(companyName))
        {
            destination.CompanyName = companyName;
        }
        
        var phone = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactMobilePhone));
        if (!string.IsNullOrWhiteSpace(phone))
        {
            destination.MobilePhone = phone;
        }
        
        var bizPhone = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactBusinessPhone));
        if (!string.IsNullOrWhiteSpace(bizPhone))
        {
            destination.Telephone1 = bizPhone;
        }
        
        var middleName = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactMiddleName));
        if (!string.IsNullOrWhiteSpace(middleName))
        {
            destination.MiddleName = middleName;
        }
        
        var jobTitle = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactJobTitle));
        if (!string.IsNullOrWhiteSpace(jobTitle))
        {
            destination.JobTitle = jobTitle;
        }
        
        var address1 = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactAddress1));
        if (!string.IsNullOrWhiteSpace(address1))
        {
            destination.Address1_Line1 = address1;
        }
        
        var city = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactCity));
        if (!string.IsNullOrWhiteSpace(city))
        {
            destination.Address1_City = city;
        }
        
        //@TODO country, state

        return Task.FromResult(destination);
    }
}