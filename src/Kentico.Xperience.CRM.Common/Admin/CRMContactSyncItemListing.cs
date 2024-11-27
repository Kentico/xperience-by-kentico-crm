using CMS.ContactManagement;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using Kentico.Xperience.CRM.Common.Admin;

[assembly: UIPage(
    parentType: typeof(ContactManagementApplication),
    slug: "crm-contact-sync-listing",
    uiPageType: typeof(CRMContactSyncItemListing),
    name: "CRM contacts sync",
    templateName: TemplateNames.LISTING,
    order: 1000)]

namespace Kentico.Xperience.CRM.Common.Admin;

internal class CRMContactSyncItemListing : ListingPage
{
    protected override string ObjectType => CRMSyncItemInfo.OBJECT_TYPE;

    public override Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityID), "Email", maxWidth: 50)
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityCRM), "CRM", maxWidth: 20)
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemCRMID), "CRM ID")
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemLastModified), "Last sync");

        PageConfiguration.QueryModifiers.AddModifier(q =>
            q.WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityClass), ContactInfo.TYPEINFO.ObjectClassName)
                .OrderByDescending(nameof(CRMSyncItemInfo.CRMSyncItemLastModified)));

        return base.ConfigurePage();
    }
}
