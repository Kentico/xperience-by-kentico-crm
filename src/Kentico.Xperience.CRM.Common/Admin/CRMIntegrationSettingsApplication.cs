using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.CRM.Common.Admin;

[assembly: UIApplication(
    identifier: CRMIntegrationSettingsApplication.IDENTIFIER,
    type: typeof(CRMIntegrationSettingsApplication),
    slug: "crm-integration",
    name: "CRM integration",
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.IntegrationScheme,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Application entry point to CRM integration settings
/// </summary>
[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.UPDATE)]
public class CRMIntegrationSettingsApplication : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.CRM.Common.IntegrationSettings";
}