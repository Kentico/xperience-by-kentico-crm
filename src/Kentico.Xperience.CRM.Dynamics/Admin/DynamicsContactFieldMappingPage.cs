using CMS.ContactManagement;
using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;
using Kentico.Xperience.CRM.Common.Metadata;
using Kentico.Xperience.CRM.Dynamics.Admin;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Metadata;

using Microsoft.Extensions.Options;

[assembly: UIPage(
    parentType: typeof(CRMIntegrationSettingsApplication),
    slug: "dynamics-contact-field-mapping",
    uiPageType: typeof(DynamicsContactFieldMappingPage),
    name: "Dynamics contact mapping",
    templateName: CRMAdminClientModule.CONTACT_FIELD_MAPPING_TEMPLATE,
    order: 100)]

namespace Kentico.Xperience.CRM.Dynamics.Admin;

/// <summary>
/// Lets marketers map Xperience contact fields onto Dataverse lead and contact fields.
/// </summary>
internal class DynamicsContactFieldMappingPage : ContactFieldMappingPage
{
    private readonly DynamicsEntityMetadataProvider metadataProvider;
    private readonly IOptionsSnapshot<DynamicsIntegrationSettings> settings;

    public DynamicsContactFieldMappingPage(IContactFieldMappingService mappingService,
        IContactFieldSourceProvider sourceProvider,
        IEnumerable<IContactSourceValueResolver> resolvers,
        IInfoProvider<ContactInfo> contactInfoProvider,
        DynamicsEntityMetadataProvider metadataProvider,
        IOptionsSnapshot<DynamicsIntegrationSettings> settings)
        : base(mappingService, sourceProvider, resolvers, contactInfoProvider)
    {
        this.metadataProvider = metadataProvider;
        this.settings = settings;
    }

    protected override string CRMName => CRMType.Dynamics;

    protected override ContactCRMType ConfiguredContactType => settings.Value.ContactType;

    protected override ICRMEntityMetadataProvider MetadataProvider => metadataProvider;

    protected override IReadOnlyList<ContactFieldMappingDefinition> GetDefaultSuggestions(string entityType) =>
        DynamicsDefaultContactMappings.Get(entityType);
}