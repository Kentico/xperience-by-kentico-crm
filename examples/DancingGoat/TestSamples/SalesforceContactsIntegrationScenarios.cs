using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Enums;

namespace DancingGoat.TestSamples;

internal static class SalesforceContactsIntegrationScenarios
{
    
    public static void InitSalesforceContactsIntegration(this WebApplicationBuilder builder)
    {
        //InitToLeadsSimple(builder);
        InitToLeadsCustomMapping(builder);
    }
    
    private static void InitToLeadsSimple(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead);
    }

    private static void InitToContactsSimple(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Contact);
    }

    private static void InitToLeadsCustomMapping(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.MapField(nameof(ContactInfo.ContactEmail), "Email")
                .MapField(c => c.ContactFirstName, "FirstName")
                .MapLeadField(nameof(ContactInfo.ContactLastName), e => e.LastName)
                .MapLeadField(c => c.ContactMobilePhone, e => e.MobilePhone),
            useDefaultMappingToCRM: false);
    }

    private static void InitToContactsCustomMapping(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Contact, builder =>
            builder.MapField(nameof(ContactInfo.ContactEmail), "Email")
                .MapField(c => c.ContactFirstName, "FirstName")
                .MapContactField(nameof(ContactInfo.ContactLastName), e => e.LastName)
                .MapContactField(c => c.ContactMobilePhone, e => e.MobilePhone),
            useDefaultMappingToCRM: false);
    }
    
    private static void InitToLeadsDefaultMappingAndCustomMapField(WebApplicationBuilder builder)
    {
        //should rewrite value for Description from default mapping
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.MapLeadField(c => $"Created in admin: {c.ContactCreatedInAdministration}", e => e.Description));
    }
    
    private static void InitToContactsDefaultMappingAndCustomMapField(WebApplicationBuilder builder)
    {
        //should rewrite value for description from default mapping
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Contact, builder =>
            builder.MapContactField(c => $"Created in admin: {c.ContactCreatedInAdministration}", e => e.Description));
    }
    
    private static void InitToLeadsCustomConverter(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.AddContactToLeadConverter<SalesforceContactToLeadCustomConverter>(),
            useDefaultMappingToCRM: false); // when true default mapping is applied after custom converter
    }
    
    private static void InitToContactsCustomConverter(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.AddContactToContactConverter<SalesforceContactToContactCustomConverter>(),
            useDefaultMappingToCRM: false); // when true default mapping is applied after custom converter
    }
    
    private static void InitToLeadsCustomConverterToKentico(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.AddLeadToKenticoConverter<SalesforceLeadToKenticoContactCustomConverter>(),
            useDefaultMappingToKentico: false); // when true then both (custom and default) converter are applied
    }
    
    private static void InitToContactsCustomConverterToKentico(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Contact, builder =>
            builder.AddContactToKenticoConverter<SalesforceContactToKenticoContactCustomConverter>(),
            useDefaultMappingToKentico: false); // when true then both (custom and default) converter are applied
    }
    
    private static void InitToLeadsComplex(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Lead, builder =>
            builder.MapField(nameof(ContactInfo.ContactEmail), "Email")
                .MapField(c => c.ContactFirstName, "FirstName")
                .MapLeadField(nameof(ContactInfo.ContactLastName), e => e.LastName)
                .MapLeadField(c => c.ContactMobilePhone, e => e.MobilePhone)
                .AddContactToLeadConverter<SalesforceContactToLeadCustomConverter>()
                .AddLeadToKenticoConverter<SalesforceLeadToKenticoContactCustomConverter>(),
                useDefaultMappingToCRM: false, useDefaultMappingToKentico: false);
    }
    
    private static void InitToContactsComplex(WebApplicationBuilder builder)
    {
        builder.Services.AddKenticoCRMSalesforceContactsIntegration(crmType: ContactCRMType.Contact, builder =>
                builder.MapField(nameof(ContactInfo.ContactEmail), "Email")
                    .MapField(c => c.ContactFirstName, "FirstName")
                    .MapContactField(nameof(ContactInfo.ContactLastName), e => e.LastName)
                    .MapContactField(c => c.ContactMobilePhone, e => e.MobilePhone)
                    .AddContactToContactConverter<SalesforceContactToContactCustomConverter>()
                    .AddContactToKenticoConverter<SalesforceContactToKenticoContactCustomConverter>(),
            useDefaultMappingToCRM: false, useDefaultMappingToKentico: false);
    }
}