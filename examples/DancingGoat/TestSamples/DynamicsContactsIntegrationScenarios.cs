using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Dynamics;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

namespace DancingGoat.TestSamples;

internal static class DynamicsContactsIntegrationScenarios
{
    public static void InitDynamicsContactsIntegration(this WebApplicationBuilder builder)
    {
        //InitToLeadsSimple(builder);
        InitToLeadsCustomMapping(builder);
    }

    private static void InitToLeadsSimple(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead);
    }

    private static void InitToContactsSimple(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Contact);
    }

    private static void InitToLeadsCustomMapping(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.MapField(nameof(ContactInfo.ContactEmail), "emailaddress1")
                .MapField(c => c.ContactFirstName, "firstname")
                .MapField<Lead>(nameof(ContactInfo.ContactLastName), e => e.LastName)
                .MapField<Lead>(c => c.ContactMobilePhone, e => e.MobilePhone),
            useDefaultMappingToCRM: false);
    }

    private static void InitToContactsCustomMapping(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Contact, builder =>
            builder.MapField(nameof(ContactInfo.ContactEmail), "emailaddress1")
                .MapField(c => c.ContactFirstName, "firstname")
                .MapField<Contact>(nameof(ContactInfo.ContactLastName), e => e.LastName)
                .MapField<Contact>(c => c.ContactMobilePhone, e => e.MobilePhone),
            useDefaultMappingToCRM: false);
    }

    private static void InitToLeadsDefaultMappingAndCustomMapField(WebApplicationBuilder builder)
    {
        //should rewrite value for Description from default mapping
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.MapField<Contact>(c => $"Created in admin: {c.ContactCreatedInAdministration}", e => e.Description));
    }

    private static void InitToContactsDefaultMappingAndCustomMapField(WebApplicationBuilder builder)
    {
        //should rewrite value for description from default mapping
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Contact, builder =>
            builder.MapField<Contact>(c => $"Created in admin: {c.ContactCreatedInAdministration}", e => e.Description));
    }

    private static void InitToLeadsCustomConverter(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.AddContactToLeadConverter<DynamicsContactToLeadCustomConverter>(),
            useDefaultMappingToCRM: false); // when true default mapping is applied after custom converter
    }

    private static void InitToContactsCustomConverter(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.AddContactToContactConverter<DynamicsContactToContactCustomConverter>(),
            useDefaultMappingToCRM: false); // when true default mapping is applied after custom converter
    }

    private static void InitToLeadsCustomConverterToKentico(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.AddLeadToKenticoConverter<DynamicsLeadToKenticoContactCustomConverter>(),
            useDefaultMappingToKentico: false); // when true then both (custom and default) converter are applied
    }

    private static void InitToContactsCustomConverterToKentico(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Contact, builder =>
            builder.AddContactToKenticoConverter<DynamicsContactToKenticoContactCustomConverter>(),
            useDefaultMappingToKentico: false); // when true then both (custom and default) converter are applied
    }

    private static void InitToLeadsComplex(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.MapField(nameof(ContactInfo.ContactEmail), "emailaddress1")
                .MapField(c => c.ContactFirstName, "firstname")
                .MapField<Lead>(nameof(ContactInfo.ContactLastName), e => e.LastName)
                .MapField<Lead>(c => c.ContactMobilePhone, e => e.MobilePhone)
                .AddContactToLeadConverter<DynamicsContactToLeadCustomConverter>()
                .AddLeadToKenticoConverter<DynamicsLeadToKenticoContactCustomConverter>(),
                useDefaultMappingToCRM: false, useDefaultMappingToKentico: false);
    }

    private static void InitToContactsComplex(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMDynamicsContactsIntegration(crmType: ContactCRMType.Contact, builder =>
           builder.MapField(nameof(ContactInfo.ContactEmail), "emailaddress1")
               .MapField(c => c.ContactFirstName, "firstname")
               .MapField<Contact>(nameof(ContactInfo.ContactLastName), e => e.LastName)
               .MapField<Contact>(c => c.ContactMobilePhone, e => e.MobilePhone)
               .AddContactToContactConverter<DynamicsContactToContactCustomConverter>()
               .AddContactToKenticoConverter<DynamicsContactToKenticoContactCustomConverter>(),
               useDefaultMappingToCRM: false, useDefaultMappingToKentico: false);
    }
}
