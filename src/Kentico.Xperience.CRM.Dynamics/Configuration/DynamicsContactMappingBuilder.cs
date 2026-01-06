using System.Linq.Expressions;

using CMS.ContactManagement;
using CMS.Globalization;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Dynamics.Converters;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Kentico.Xperience.CRM.Dynamics.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Xrm.Sdk;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

public class DynamicsContactMappingBuilder : ContactMappingBuilder<DynamicsContactMappingBuilder>
{
    private readonly IServiceCollection serviceCollection;

    public DynamicsContactMappingBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public DynamicsContactMappingBuilder MapField<TCRMEntity>(string contactFieldName,
        Expression<Func<TCRMEntity, object>> expression)
        where TCRMEntity : Entity
    {
        string crmFieldName = EntityHelper.GetLogicalNameFromExpression(expression);
        if (crmFieldName == string.Empty)
        {
            throw new InvalidOperationException("Attribute name cannot be empty");
        }

        MapField(contactFieldName, crmFieldName);
        return this;
    }

    public DynamicsContactMappingBuilder MapField<TCRMEntity>(
        Func<ContactInfo, object> contactInfoMappingFunc, Expression<Func<TCRMEntity, object>> expression)
        where TCRMEntity : Entity
    {
        string crmFieldName = EntityHelper.GetLogicalNameFromExpression(expression);

        if (crmFieldName == string.Empty)
        {
            throw new InvalidOperationException("Attribute name cannot be empty");
        }

        return MapField(contactInfoMappingFunc, crmFieldName);
    }

    public DynamicsContactMappingBuilder AddDefaultMappingForLead()
    {
        MapField<Lead>(c => c.ContactFirstName, l => l.FirstName);
        MapField<Lead>(c => c.ContactMiddleName, l => l.MiddleName);
        MapField<Lead>(c => c.ContactLastName, l => l.LastName);
        MapField<Lead>(c => c.ContactEmail, l => l.EMailAddress1);
        MapField<Lead>(c => c.ContactAddress1, l => l.Address1_Line1);
        MapField<Lead>(c => c.ContactCity, l => l.Address1_City);
        MapField<Lead>(c => c.ContactZIP, l => l.Address1_PostalCode);
        MapField<Lead>(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.Address1_Country);
        MapField<Lead>(c => c.ContactJobTitle, l => l.JobTitle);
        MapField<Lead>(c => c.ContactMobilePhone, l => l.MobilePhone);
        MapField<Lead>(c => c.ContactBusinessPhone, l => l.Telephone1);
        MapField<Lead>(c => c.ContactCompanyName, l => l.CompanyName);
        MapField<Lead>(c => c.ContactNotes, l => l.Description);

        return this;
    }

    public DynamicsContactMappingBuilder AddDefaultMappingForContact()
    {
        MapField<Contact>(c => c.ContactFirstName, l => l.FirstName);
        MapField<Contact>(c => c.ContactMiddleName, l => l.MiddleName);
        MapField<Contact>(c => c.ContactLastName, l => l.LastName);
        MapField<Contact>(c => c.ContactEmail, l => l.EMailAddress1);
        MapField<Contact>(c => c.ContactAddress1, l => l.Address1_Line1);
        MapField<Contact>(c => c.ContactCity, l => l.Address1_City);
        MapField<Contact>(c => c.ContactZIP, l => l.Address1_PostalCode);
        MapField<Contact>(
            c => c.ContactCountryID > 0
                ? CountryInfo.Provider.Get(c.ContactCountryID)?.CountryDisplayName ?? string.Empty
                : string.Empty, l => l.Address1_Country);
        MapField<Contact>(c => c.ContactJobTitle, l => l.JobTitle);
        MapField<Contact>(c => c.ContactMobilePhone, l => l.MobilePhone);
        MapField<Contact>(c => c.ContactBusinessPhone, l => l.Telephone1);
        MapField<Contact>(c => c.ContactNotes, l => l.Description);

        return this;
    }

    public DynamicsContactMappingBuilder AddContactToLeadConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<ContactInfo, Lead>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<ContactInfo, Lead>, TConverter>());
        return this;
    }

    public DynamicsContactMappingBuilder AddContactToContactConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<ContactInfo, Contact>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<ContactInfo, Contact>, TConverter>());
        return this;
    }

    public DynamicsContactMappingBuilder AddDefaultMappingToKenticoContact()
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<Lead, ContactInfo>, LeadToKenticoContactConverter>());
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<Contact, ContactInfo>, ContactToKenticoContactConverter>());

        return this;
    }

    public DynamicsContactMappingBuilder AddLeadToKenticoConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<Lead, ContactInfo>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<Lead, ContactInfo>, TConverter>());

        return this;
    }

    public DynamicsContactMappingBuilder AddContactToKenticoConverter<TConverter>()
        where TConverter : class, ICRMTypeConverter<Contact, ContactInfo>
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<Contact, ContactInfo>, TConverter>());

        return this;
    }

    public DynamicsContactMappingConfiguration Build() =>
        new() { FieldsMapping = fieldMappings };
}