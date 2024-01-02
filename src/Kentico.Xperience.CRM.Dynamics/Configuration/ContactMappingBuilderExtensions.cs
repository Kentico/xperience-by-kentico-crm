using CMS.ContactManagement;
using CMS.Globalization;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Kentico.Xperience.CRM.Dynamics.Helpers;
using Microsoft.Xrm.Sdk;
using System.Linq.Expressions;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

public static class ContactMappingBuilderExtensions
{
    public static ContactMappingBuilder MapField<TCRMEntity>(this ContactMappingBuilder builder,
        string contactFieldName,
        Expression<Func<TCRMEntity, object>> expression)
        where TCRMEntity : Entity
    {
        string crmFieldName = EntityHelper.GetLogicalNameFromExpression(expression);
        if (crmFieldName == string.Empty)
        {
            throw new InvalidOperationException("Attribute name cannot be empty");
        }

        builder.MapField(contactFieldName, crmFieldName);
        return builder;
    }

    public static ContactMappingBuilder MapField<TCRMEntity>(this ContactMappingBuilder builder,
        Func<ContactInfo, object> contactInfoMappingFunc, Expression<Func<TCRMEntity, object>> expression)
        where TCRMEntity : Entity
    {
        string crmFieldName = EntityHelper.GetLogicalNameFromExpression(expression);

        if (crmFieldName == string.Empty)
        {
            throw new InvalidOperationException("Attribute name cannot be empty");
        }

        return builder.MapField(contactInfoMappingFunc, crmFieldName);
    }

    public static ContactMappingBuilder AddDefaultMappingForLead(this ContactMappingBuilder builder)
    {
        builder.MapField<Lead>(c => c.ContactFirstName, l => l.FirstName);
        builder.MapField<Lead>(c => c.ContactMiddleName, l => l.MiddleName);
        builder.MapField<Lead>(c => c.ContactLastName, l => l.LastName);
        builder.MapField<Lead>(c => c.ContactEmail, l => l.EMailAddress1);
        builder.MapField<Lead>(c => c.ContactAddress1, l => l.Address1_Line1);
        builder.MapField<Lead>(c => c.ContactCity, l => l.Address1_City);
        builder.MapField<Lead>(c => c.ContactZIP, l => l.Address1_PostalCode);
        builder.MapField<Lead>(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.Address1_Country);
        builder.MapField<Lead>(c => c.ContactJobTitle, l => l.JobTitle);
        builder.MapField<Lead>(c => c.ContactMobilePhone, l => l.MobilePhone);
        builder.MapField<Lead>(c => c.ContactBusinessPhone, l => l.Telephone1);
        builder.MapField<Lead>(c => c.ContactCompanyName, l => l.CompanyName);
        builder.MapField<Lead>(c => c.ContactNotes, l => l.Description);
        
        return builder;
    }
    
    public static ContactMappingBuilder AddDefaultMappingForContact(this ContactMappingBuilder builder)
    {
        builder.MapField<Contact>(c => c.ContactFirstName, l => l.FirstName);
        builder.MapField<Contact>(c => c.ContactMiddleName, l => l.MiddleName);
        builder.MapField<Contact>(c => c.ContactLastName, l => l.LastName);
        builder.MapField<Contact>(c => c.ContactEmail, l => l.EMailAddress1);
        builder.MapField<Contact>(c => c.ContactAddress1, l => l.Address1_Line1);
        builder.MapField<Contact>(c => c.ContactCity, l => l.Address1_City);
        builder.MapField<Contact>(c => c.ContactZIP, l => l.Address1_PostalCode);
        builder.MapField<Contact>(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.Address1_Country);
        builder.MapField<Contact>(c => c.ContactJobTitle, l => l.JobTitle);
        builder.MapField<Contact>(c => c.ContactMobilePhone, l => l.MobilePhone);
        builder.MapField<Contact>(c => c.ContactBusinessPhone, l => l.Telephone1);
        builder.MapField<Contact>(c => c.ContactCompanyName, l => l.Company);
        builder.MapField<Contact>(c => c.ContactNotes, l => l.Description);
        
        return builder;
    }
}