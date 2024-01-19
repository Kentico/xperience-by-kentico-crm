using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.CRM.SalesForce.Admin;

[assembly: UIApplication(
    identifier: SalesForceIntegrationSettingApplication.IDENTIFIER, 
    type: typeof(SalesForceIntegrationSettingApplication),
    slug: "salesforce-settings",
    name: "SalesForce CRM Integration Settings", 
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.IntegrationScheme, 
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.CRM.SalesForce.Admin;

internal class SalesForceIntegrationSettingApplication : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.CRM.SalesForce.IntegrationSettings";
}