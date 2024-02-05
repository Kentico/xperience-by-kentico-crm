using CMS.ContactManagement;
using CMS.Globalization;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Salesforce.OpenApi;
using System.Linq.Expressions;

namespace Kentico.Xperience.CRM.Salesforce.Configuration;

public class SalesforceContactMappingBuilder : ContactMappingBuilder<SalesforceContactMappingBuilder>
{
    private readonly IServiceCollection serviceCollection;

    public SalesforceContactMappingBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public SalesforceContactMappingBuilder MapLeadField(string contactFieldName,
        Expression<Func<LeadSObject, object?>> expression)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldNameMapping(contactFieldName),
            new CRMFieldMappingFunction<LeadSObject>(expression)));
        return this;
    }

    public SalesforceContactMappingBuilder MapLeadField(
        Func<ContactInfo, object> contactInfoMappingFunc,
        Expression<Func<LeadSObject, object?>> expression)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldMappingFunction(contactInfoMappingFunc),
            new CRMFieldMappingFunction<LeadSObject>(expression)));
        return this;
    }

    public SalesforceContactMappingBuilder MapContactField(string contactFieldName,
        Expression<Func<ContactSObject, object?>> expression)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldNameMapping(contactFieldName),
            new CRMFieldMappingFunction<ContactSObject>(expression)));
        return this;
    }

    public SalesforceContactMappingBuilder MapContactField(
        Func<ContactInfo, object> contactInfoMappingFunc,
        Expression<Func<ContactSObject, object?>> expression)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldMappingFunction(contactInfoMappingFunc),
            new CRMFieldMappingFunction<ContactSObject>(expression)));
        return this;
    }

    public SalesforceContactMappingBuilder AddDefaultMappingForLead()
    {
        MapLeadField(c => c.ContactFirstName, l => l.FirstName);
        MapLeadField(c => c.ContactLastName, l => l.LastName);
        MapLeadField(c => c.ContactEmail, l => l.Email);
        MapLeadField(c => c.ContactAddress1, l => l.Street);
        MapLeadField(c => c.ContactCity, l => l.City);
        MapLeadField(c => c.ContactZIP, l => l.PostalCode);
        MapLeadField(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.Country);
        MapLeadField(c => c.ContactMobilePhone, l => l.MobilePhone);
        MapLeadField(c => c.ContactBusinessPhone, l => l.Phone);
        MapLeadField(c => c.ContactCompanyName, l => l.Company);
        MapLeadField(c => c.ContactNotes, l => l.Description);
        
        return this;
    }
    
    public SalesforceContactMappingBuilder AddDefaultMappingForContact()
    {
        MapContactField(c => c.ContactFirstName, l => l.FirstName);
        MapContactField(c => c.ContactLastName, l => l.LastName);
        MapContactField(c => c.ContactEmail, l => l.Email);
        MapContactField(c => c.ContactAddress1, l => l.MailingStreet);
        MapContactField(c => c.ContactCity, l => l.MailingCity);
        MapContactField(c => c.ContactZIP, l => l.MailingPostalCode);
        MapContactField(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.MailingCountry);
        MapContactField(c => c.ContactMobilePhone, l => l.MobilePhone);
        MapContactField(c => c.ContactBusinessPhone, l => l.Phone);
        MapContactField(c => c.ContactNotes, l => l.Description);
        
        return this;
    }
    
    public SalesforceContactMappingBuilder AddContactToLeadConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<ContactInfo, LeadSObject>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<ContactInfo, LeadSObject>, TConverter>());
        return this;
    }
    
    public SalesforceContactMappingBuilder AddContactToContactConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<ContactInfo, ContactSObject>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<ContactInfo, ContactSObject>, TConverter>());
        return this;
    }
    
    public SalesforceContactMappingBuilder AddDefaultMappingToKenticoContact()
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<LeadSObject, ContactInfo>, LeadToKenticoContactConverter>());
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<ContactSObject, ContactInfo>, ContactToKenticoContactConverter>());
        
        return this;
    }
    
    public SalesforceContactMappingBuilder AddLeadToKenticoConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<LeadSObject, ContactInfo>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<LeadSObject, ContactInfo>, TConverter>());
        
        return this;
    }
    
    public SalesforceContactMappingBuilder AddContactToKenticoConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<ContactSObject, ContactInfo>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<ContactSObject, ContactInfo>, TConverter>());
        
        return this;
    }
    
    internal SalesforceContactMappingConfiguration Build() =>
        new()
        {
            FieldsMapping = fieldMappings
        };
}