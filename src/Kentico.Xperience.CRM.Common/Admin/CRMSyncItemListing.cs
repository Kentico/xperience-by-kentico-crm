using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.OnlineForms.Internal;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using Kentico.Xperience.CRM.Common.Admin;

[assembly: UIPage(
    parentType: typeof(FormEditSection),
    slug: "crm-sync-listing",
    uiPageType: typeof(CRMSyncItemListing),
    name: "CRM sync",
    templateName: TemplateNames.LISTING,
    order: 1000,
    Icon = Icons.IntegrationScheme)]

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Admin listing page for displaying synced items in CMS for selected form
/// </summary>
internal class CRMSyncItemListing : ListingPage
{
    private readonly IContactFieldFromFormRetriever contactFieldFromFormRetriever;
    private BizFormInfo? editedForm;
    private DataClassInfo? dataClassInfo;
    protected override string ObjectType => CRMSyncItemInfo.OBJECT_TYPE;

    /// <summary>ID of the edited form.</summary>
    [PageParameter(typeof(IntPageModelBinder), typeof(FormEditSection))]
    public int FormId { get; set; }

    private BizFormInfo EditedForm =>
        this.editedForm ??= AbstractInfo<BizFormInfo, IInfoProvider<BizFormInfo>>.Provider.Get(FormId);

    private DataClassInfo DataClassInfo => this.dataClassInfo ??=
        DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(EditedForm.FormClassID);

    public CRMSyncItemListing(IContactFieldFromFormRetriever contactFieldFromFormRetriever)
    {
        this.contactFieldFromFormRetriever = contactFieldFromFormRetriever;
    }

    public override Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityID), formatter: (value, _) =>
            {
                var bizFormItem =
                    BizFormItemProvider.GetItem(ValidationHelper.GetInteger(value, 0), DataClassInfo.ClassName);
                // first try to get email by mapping to Contact email
                var email = contactFieldFromFormRetriever.RetrieveContactEmail(bizFormItem);
                if (email is not null)
                {
                    return email;
                }

                //secondary try to find email field from field with 'Email' in his name
                var emailField = new FormInfo(DataClassInfo.ClassFormDefinition).ItemsList.OfType<FormFieldInfo>()
                    .FirstOrDefault(fi =>
                        fi.Name.Contains("email", StringComparison.InvariantCultureIgnoreCase) && fi.DataType == "text")
                    ?.Name;
                return bizFormItem.GetStringValue(emailField, string.Empty);
            }, caption: "Email")
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityID), "Form item ID", maxWidth: 20)
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemEntityCRM), "CRM", maxWidth: 20)
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemCRMID), "CRM ID")
            .AddColumn(nameof(CRMSyncItemInfo.CRMSyncItemLastModified), "Last sync");

        PageConfiguration.QueryModifiers.AddModifier(q =>
            q.WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityClass), DataClassInfo.ClassName)
                .OrderByDescending(nameof(CRMSyncItemInfo.CRMSyncItemLastModified)));

        return base.ConfigurePage();
    }
}