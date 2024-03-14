using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Model for CRM integration settings in Admin
/// </summary>
public class CRMIntegrationSettingsModel
{
    [CheckBoxComponent(Label = "Forms enabled", Order = 1)]
    public bool FormsEnabled { get; set; }

    [CheckBoxComponent(Label = "Contacts enabled", Order = 2)]
    public bool ContactsEnabled { get; set; }

    [CheckBoxComponent(Label = "Contacts two-way sync enabled", Order = 3)]
    [VisibleIfTrue(nameof(ContactsEnabled))]
    public bool ContactsTwoWaySyncEnabled { get; set; } = true;

    [CheckBoxComponent(Label = "Ignore existing records", Order = 4)]
    public bool IgnoreExistingRecords { get; set; }

    [UrlValidationRule]
    [TextInputComponent(Label = "CRM URL", Order = 5)]
    [RequiredValidationRule]
    public string? Url { get; set; }

    [RequiredValidationRule]
    [TextInputComponent(Label = "Client ID", Order = 6)]
    public string? ClientId { get; set; }

    [RequiredValidationRule]
    [PasswordComponent(Label = "Client secret", Order = 7, RequiredLength = 0, RequireDigit = false,
        RequireLowercase = false, RequireUppercase = false, RequiredUniqueChars = 0, RequireNonAlphanumeric = false)]
    public string? ClientSecret { get; set; }
}