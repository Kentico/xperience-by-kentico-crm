using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.CRM.Dynamics.Admin;

[assembly: UIApplication(
    identifier: DynamicsIntegrationSettingsApplication.IDENTIFIER, 
    type: typeof(DynamicsIntegrationSettingsApplication),
    slug: "dynamics-settings",
    name: "Dynamics CRM Integration Settings", 
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.IntegrationScheme, 
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.CRM.Dynamics.Admin;

internal class DynamicsIntegrationSettingsApplication : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.CRM.Dynamics.IntegrationSettings";
}