using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.CRM.Common.Classes;
using Kentico.Xperience.CRM.Dynamics.Admin;

[assembly: UIPage(
    parentType: typeof(DynamicsIntegrationSettingsApplication),
    slug: "edit", 
    uiPageType: typeof(DynamicsIntegrationSettingsEdit), 
    name: "Edit settings", 
    templateName: TemplateNames.EDIT,
    order:UIPageOrder.First)]

namespace Kentico.Xperience.CRM.Dynamics.Admin;

public class DynamicsIntegrationSettingsEdit : InfoEditPage<CRMIntegrationSettingsInfo>
{
    public DynamicsIntegrationSettingsEdit(IFormComponentMapper formComponentMapper, IFormDataBinder formDataBinder) :
        base(formComponentMapper, formDataBinder)
    {
    }

    public override int ObjectId { get; set; } = 1; //just 1 settings in DB (with ID 1)
}