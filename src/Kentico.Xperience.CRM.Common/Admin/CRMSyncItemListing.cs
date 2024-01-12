using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Classes;

[assembly:
    UIPage(typeof(FormEditSection), "crm-sync-listing", typeof(CRMSyncItemListing),
        "CRM synchronization", TemplateNames.LISTING, 1000, Icons.IntegrationScheme)]

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Admin listing page for displaying synced items in CMS for selected form
/// </summary>
public class CRMSyncItemListing : ListingPage
{
    private BizFormInfo? editedForm;
    private DataClassInfo? dataClassInfo;
    protected override string ObjectType => CRMSyncItemInfo.OBJECT_TYPE;

    /// <summary>ID of the edited form.</summary>
    [PageParameter(typeof(IntPageModelBinder), typeof(FormEditSection))]
    public int FormId { get; set; }

    private BizFormInfo EditedForm =>
        this.editedForm ??= AbstractInfo<BizFormInfo, IBizFormInfoProvider>.Provider.Get(FormId);

    private DataClassInfo DataClassInfo => this.dataClassInfo ??=
        DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(EditedForm.FormClassID);

    public override Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityID), "Form item ID")
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityCRM), "CRM")
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemCRMID), "CRM ID")
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemLastModified), "Last sync time");

        PageConfiguration.QueryModifiers.AddModifier(q =>
            q.WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityClass), DataClassInfo.ClassName)
                .OrderByDescending(nameof(CRMSyncItemInfo.CRMSyncItemLastModified)));

        return Task.CompletedTask;
    }
}