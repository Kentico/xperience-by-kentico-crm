using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.OnlineForms;
using CMS.OnlineForms.Internal;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Converters;

/// <summary>
/// Converter for mapping BizForm to Lead based on Form-Contact mapping in CMS
/// </summary>
public class FormContactMappingToLeadConverter : ICRMTypeConverter<BizFormItem, LeadSObject>
{
    private readonly IContactFieldFromFormRetriever contactFieldFromFormRetriever;
    private readonly ICountryInfoProvider countries;
    private readonly IStateInfoProvider states;
    private readonly IConversionService conversion;

    public FormContactMappingToLeadConverter(
        IContactFieldFromFormRetriever contactFieldFromFormRetriever,
        ICountryInfoProvider countries,
        IStateInfoProvider states,
        IConversionService conversion)
    {
        this.contactFieldFromFormRetriever = contactFieldFromFormRetriever;
        this.countries = countries;
        this.states = states;
        this.conversion = conversion;
    }

    public Task Convert(BizFormItem source, LeadSObject destination)
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

        string countryIDVal = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactCountryID));
        if (!string.IsNullOrWhiteSpace(countryIDVal))
        {
            var country = countries.Get(conversion.GetInteger(countryIDVal, 0));
            destination.Country = country?.CountryDisplayName;
        }

        string stateIDVal = contactFieldFromFormRetriever.Retrieve(source, nameof(ContactInfo.ContactStateID));
        if (!string.IsNullOrWhiteSpace(stateIDVal))
        {
            var state = states.Get(conversion.GetInteger(stateIDVal, 0));
            destination.State = state?.StateDisplayName;
        }

        return Task.CompletedTask;
    }
}