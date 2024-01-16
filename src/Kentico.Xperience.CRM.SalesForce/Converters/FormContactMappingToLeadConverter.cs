using CMS.ContactManagement;
using CMS.OnlineForms;
using CMS.OnlineForms.Internal;
using Kentico.Xperience.CRM.Common.Mapping;
using SalesForce.OpenApi;

namespace Kentico.Xperience.CRM.SalesForce.Converters;

/// <summary>
/// Converter for mapping BizForm to Lead based on Form-Contact mapping in CMS
/// </summary>
public class FormContactMappingToLeadConverter : ICRMTypeConverter<BizFormItem, LeadSObject>
{
    private readonly IContactFieldFromFormRetriever contactFieldFromFormRetriever;

    public FormContactMappingToLeadConverter(IContactFieldFromFormRetriever contactFieldFromFormRetriever)
    {
        this.contactFieldFromFormRetriever = contactFieldFromFormRetriever;
    }

    public Task<LeadSObject> Convert(BizFormItem source, LeadSObject destination)
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
            destination.Email = email;
        }
        
        var companyName = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactCompanyName));
        if (!string.IsNullOrWhiteSpace(companyName))
        {
            destination.Company = companyName;
        }
        
        var phone = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactMobilePhone));
        if (!string.IsNullOrWhiteSpace(phone))
        {
            destination.MobilePhone = phone;
        }
        
        var bizPhone = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactBusinessPhone));
        if (!string.IsNullOrWhiteSpace(bizPhone))
        {
            destination.Phone = bizPhone;
        }
        
        var jobTitle = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactJobTitle));
        if (!string.IsNullOrWhiteSpace(jobTitle))
        {
            destination.Title = jobTitle;
        }
        
        var address1 = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactAddress1));
        if (!string.IsNullOrWhiteSpace(address1))
        {
            destination.Street = address1;
        }
        
        var city = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactCity));
        if (!string.IsNullOrWhiteSpace(city))
        {
            destination.City = city;
        }
        
        var zipCode = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactZIP));
        if (!string.IsNullOrWhiteSpace(zipCode))
        {
            destination.PostalCode = zipCode;
        }
        
        //@TODO country, state

        return Task.FromResult(destination);
    }
}