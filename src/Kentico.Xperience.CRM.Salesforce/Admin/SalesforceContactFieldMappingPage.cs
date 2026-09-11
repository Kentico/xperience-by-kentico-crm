using CMS.ContactManagement;
using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;
using Kentico.Xperience.CRM.Common.Metadata;
using Kentico.Xperience.CRM.Salesforce.Admin;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Kentico.Xperience.CRM.Salesforce.Metadata;

using Microsoft.Extensions.Options;

[assembly: UIPage(
    parentType: typeof(CRMIntegrationSettingsApplication),
    slug: "salesforce-contact-field-mapping",
    uiPageType: typeof(SalesforceContactFieldMappingPage),
    name: "Salesforce contact mapping",
    templateName: CRMAdminClientModule.CONTACT_FIELD_MAPPING_TEMPLATE,
    order: 400)]

namespace Kentico.Xperience.CRM.Salesforce.Admin;

/// <summary>
/// Lets marketers map Xperience contact fields onto Salesforce lead and contact fields.
/// </summary>
internal class SalesforceContactFieldMappingPage : ContactFieldMappingPage
{
    private readonly SalesforceEntityMetadataProvider metadataProvider;
    private readonly IOptionsSnapshot<SalesforceIntegrationSettings> settings;

    public SalesforceContactFieldMappingPage(IContactFieldMappingService mappingService,
        IContactFieldSourceProvider sourceProvider,
        IEnumerable<IContactSourceValueResolver> resolvers,
        IInfoProvider<ContactInfo> contactInfoProvider,
        SalesforceEntityMetadataProvider metadataProvider,
        IOptionsSnapshot<SalesforceIntegrationSettings> settings)
        : base(mappingService, sourceProvider, resolvers, contactInfoProvider)
    {
        this.metadataProvider = metadataProvider;
        this.settings = settings;
    }

    protected override string CRMName => CRMType.Salesforce;

    protected override ContactCRMType ConfiguredContactType => settings.Value.ContactType;

    protected override ICRMEntityMetadataProvider MetadataProvider => metadataProvider;

    protected override IReadOnlyList<ContactFieldMappingDefinition> GetDefaultSuggestions(string entityType) =>
        SalesforceDefaultContactMappings.Get(entityType);
}