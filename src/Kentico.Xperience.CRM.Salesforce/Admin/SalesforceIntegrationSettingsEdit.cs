using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Salesforce.Admin;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(CRMIntegrationSettingsApplication),
    slug: "salesforce-settings-edit",
    uiPageType: typeof(SalesforceIntegrationSettingsEdit),
    name: "Salesforce CRM",
    templateName: TemplateNames.EDIT,
    order: 200)]

namespace Kentico.Xperience.CRM.Salesforce.Admin;

internal class SalesforceIntegrationSettingsEdit : CRMIntegrationSettingsEdit
{
    public SalesforceIntegrationSettingsEdit(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder, ICRMIntegrationSettingsInfoProvider crmIntegrationSettingsInfoProvider) : base(
        formItemCollectionProvider, formDataBinder, crmIntegrationSettingsInfoProvider)
    {
    }

    protected override string CRMName => CRMType.Salesforce;
}