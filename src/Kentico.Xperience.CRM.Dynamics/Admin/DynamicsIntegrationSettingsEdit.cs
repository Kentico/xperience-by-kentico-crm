﻿using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Classes;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Dynamics.Admin;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(DynamicsIntegrationSettingsApplication),
    slug: "edit",
    uiPageType: typeof(DynamicsIntegrationSettingsEdit),
    name: "Edit settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace Kentico.Xperience.CRM.Dynamics.Admin;

internal class DynamicsIntegrationSettingsEdit : CRMIntegrationSettingsEdit
{
    public DynamicsIntegrationSettingsEdit(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder, ICRMIntegrationSettingsInfoProvider crmIntegrationSettingsInfoProvider) : base(
        formItemCollectionProvider, formDataBinder, crmIntegrationSettingsInfoProvider)
    {
    }

    protected override string CRMName => CRMType.Dynamics;
}