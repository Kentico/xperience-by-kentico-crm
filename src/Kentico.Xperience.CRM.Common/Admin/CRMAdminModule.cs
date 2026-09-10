using CMS;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.CRM.Common.Admin;

[assembly: RegisterModule(typeof(CRMAdminModule))]

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Identifiers of the client module which provides the custom admin UI templates of the CRM integration.
/// The organization and project names must match <c>AdminOrgName</c> and the <c>ProjectName</c> metadata of
/// <c>AdminClientPath</c> in the project file, and <c>orgName</c>/<c>projectName</c> in the webpack
/// configuration - the admin uses them to resolve the module at runtime.
/// </summary>
public static class CRMAdminClientModule
{
    /// <summary>
    /// Organization part of the client module identifier.
    /// </summary>
    public const string ORGANIZATION_NAME = "kentico";

    /// <summary>
    /// Project part of the client module identifier.
    /// </summary>
    public const string PROJECT_NAME = "xperience-integrations-crm";

    /// <summary>
    /// Template of the contact field mapping page. Resolves to the <c>ContactFieldMappingTemplate</c>
    /// component exported from the client module entry point.
    /// </summary>
    public const string CONTACT_FIELD_MAPPING_TEMPLATE =
        $"@{ORGANIZATION_NAME}/{PROJECT_NAME}/ContactFieldMapping";
}

/// <summary>
/// Makes the CRM integration client module available to the admin UI.
/// </summary>
internal class CRMAdminModule : AdminModule
{
    public CRMAdminModule()
        : base("Kentico.Xperience.CRM.Common.Admin")
    {
    }

    protected override void OnInit()
    {
        base.OnInit();

        RegisterClientModule(CRMAdminClientModule.ORGANIZATION_NAME, CRMAdminClientModule.PROJECT_NAME);
    }
}