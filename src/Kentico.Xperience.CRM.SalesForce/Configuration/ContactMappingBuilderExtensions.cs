using CMS.ContactManagement;
using CMS.Globalization;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using SalesForce.OpenApi;
using System.Linq.Expressions;

namespace Kentico.Xperience.CRM.SalesForce.Configuration;

public static class ContactMappingBuilderExtensions
{
    public static ContactMappingBuilder MapLeadField(this ContactMappingBuilder builder, string contactFieldName,
        Expression<Func<LeadSObject, object?>> expression)
    {
        return builder.AddMapping(new ContactFieldToCRMMapping(new ContactFieldNameMapping(contactFieldName),
            new CRMFieldMappingFunction<LeadSObject>(expression)));
    }

    public static ContactMappingBuilder MapLeadField(this ContactMappingBuilder builder,
        Func<ContactInfo, object> contactInfoMappingFunc,
        Expression<Func<LeadSObject, object?>> expression)
    {
        return builder.AddMapping(new ContactFieldToCRMMapping(new ContactFieldMappingFunction(contactInfoMappingFunc),
            new CRMFieldMappingFunction<LeadSObject>(expression)));
    }

    public static ContactMappingBuilder MapContactField(this ContactMappingBuilder builder, string contactFieldName,
        Expression<Func<ContactSObject, object?>> expression)
    {
        return builder.AddMapping(new ContactFieldToCRMMapping(new ContactFieldNameMapping(contactFieldName),
            new CRMFieldMappingFunction<ContactSObject>(expression)));
    }

    public static ContactMappingBuilder MapContactField(this ContactMappingBuilder builder,
        Func<ContactInfo, object> contactInfoMappingFunc,
        Expression<Func<ContactSObject, object?>> expression)
    {
        return builder.AddMapping(new ContactFieldToCRMMapping(new ContactFieldMappingFunction(contactInfoMappingFunc),
            new CRMFieldMappingFunction<ContactSObject>(expression)));
    }

    public static ContactMappingBuilder AddDefaultMappingForLead(this ContactMappingBuilder builder)
    {
        builder.MapLeadField(c => c.ContactFirstName, l => l.FirstName);
        builder.MapLeadField(c => c.ContactLastName, l => l.LastName);
        builder.MapLeadField(c => c.ContactEmail, l => l.Email);
        builder.MapLeadField(c => c.ContactAddress1, l => l.Street);
        builder.MapLeadField(c => c.ContactCity, l => l.City);
        builder.MapLeadField(c => c.ContactZIP, l => l.PostalCode);
        builder.MapLeadField(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.Country);
        builder.MapLeadField(c => c.ContactMobilePhone, l => l.MobilePhone);
        builder.MapLeadField(c => c.ContactBusinessPhone, l => l.Phone);
        builder.MapLeadField(c => c.ContactCompanyName, l => l.Company);
        builder.MapLeadField(c => c.ContactNotes, l => l.Description);
        
        return builder;
    }
    
    public static ContactMappingBuilder AddDefaultMappingForContact(this ContactMappingBuilder builder)
    {
        builder.MapContactField(c => c.ContactFirstName, l => l.FirstName);
        builder.MapContactField(c => c.ContactLastName, l => l.LastName);
        builder.MapContactField(c => c.ContactEmail, l => l.Email);
        builder.MapContactField(c => c.ContactAddress1, l => l.MailingStreet);
        builder.MapContactField(c => c.ContactCity, l => l.MailingCity);
        builder.MapContactField(c => c.ContactZIP, l => l.MailingPostalCode);
        builder.MapContactField(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.MailingCountry);
        builder.MapContactField(c => c.ContactMobilePhone, l => l.MobilePhone);
        builder.MapContactField(c => c.ContactBusinessPhone, l => l.Phone);
        builder.MapContactField(c => c.ContactNotes, l => l.Description);
        
        return builder;
    }
}