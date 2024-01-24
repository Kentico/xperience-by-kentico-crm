using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.CRM.Common.Models;

/// <summary>
/// Model for CRM integration settings in Admin
/// </summary>
public class CRMIntegrationSettingsModel
{
    [CheckBoxComponent(Label = "Forms enabled", Order = 1)]
    public bool FormsEnabled { get; set; }

    [CheckBoxComponent(Label = "Contacts enabled", Order = 2)]
    public bool ContactsEnabled { get; set; }

    [CheckBoxComponent(Label = "Ignore existing records", Order = 3)]
    public bool IgnoreExistingRecords { get; set; }

    [UrlValidationRule]
    [TextInputComponent(Label = "CRM URL", Order = 4)]
    [RequiredValidationRule]
    public string? Url { get; set; }

    [RequiredValidationRule]
    [TextInputComponent(Label = "Client ID", Order = 5)]
    public string? ClientId { get; set; }

    [RequiredValidationRule]
    [PasswordComponent(Label = "Client secret", Order = 6, RequiredLength = 0, RequireDigit = false,
        RequireLowercase = false, RequireUppercase = false, RequiredUniqueChars = 0, RequireNonAlphanumeric = false)]
    public string? ClientSecret { get; set; }
}