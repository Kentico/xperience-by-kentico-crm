using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Classes;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.SalesForce.Admin;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(SalesForceIntegrationSettingApplication),
    slug: "edit",
    uiPageType: typeof(SalesForceIntegrationSettingsEdit),
    name: "Edit settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace Kentico.Xperience.CRM.SalesForce.Admin;

internal class SalesForceIntegrationSettingsEdit : CRMIntegrationSettingsEdit
{
    public SalesForceIntegrationSettingsEdit(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder, ICRMIntegrationSettingsInfoProvider crmIntegrationSettingsInfoProvider) : base(
        formItemCollectionProvider, formDataBinder, crmIntegrationSettingsInfoProvider)
    {
    }

    protected override string CRMName => CRMType.SalesForce;
}