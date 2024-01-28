using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Base edit page for Admin CRM integration setting
/// </summary>
public abstract class CRMIntegrationSettingsEdit : ModelEditPage<CRMIntegrationSettingsModel>
{
    private readonly ICRMIntegrationSettingsInfoProvider crmIntegrationSettingsInfoProvider;

    protected CRMIntegrationSettingsEdit(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ICRMIntegrationSettingsInfoProvider crmIntegrationSettingsInfoProvider) : base(formItemCollectionProvider,
        formDataBinder)
    {
        this.crmIntegrationSettingsInfoProvider = crmIntegrationSettingsInfoProvider;
    }

    private CRMIntegrationSettingsInfo? settingsInfo;

    private CRMIntegrationSettingsInfo? SettingsInfo => settingsInfo ??= crmIntegrationSettingsInfoProvider.Get()
        .WhereEquals(nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsCRMName), CRMName)
        .TopN(1)
        .FirstOrDefault();

    protected override Task<ICommandResponse> ProcessFormData(CRMIntegrationSettingsModel model,
        ICollection<IFormItem> formItems)
    {
        var info = SettingsInfo ?? new CRMIntegrationSettingsInfo();
        info.CRMIntegrationSettingsFormsEnabled = model.FormsEnabled;
        info.CRMIntegrationSettingsContactsEnabled = model.ContactsEnabled;
        info.CRMIntegrationSettingsContactsTwoWaySyncEnabled = model.ContactsTwoWaySyncEnabled;
        info.CRMIntegrationSettingsIgnoreExistingRecords = model.IgnoreExistingRecords;
        info.CRMIntegrationSettingsClientId = model.ClientId;
        info.CRMIntegrationSettingsClientSecret = model.ClientSecret;
        info.CRMIntegrationSettingsUrl = model.Url;
        info.CRMIntegrationSettingsCRMName = CRMName;

        crmIntegrationSettingsInfoProvider.Set(info);

        return base.ProcessFormData(model, formItems);
    }

    private CRMIntegrationSettingsModel? model;
    protected override CRMIntegrationSettingsModel Model => model ??= SettingsInfo is null ?
        new CRMIntegrationSettingsModel() :
        new CRMIntegrationSettingsModel
        {
            FormsEnabled = SettingsInfo.CRMIntegrationSettingsFormsEnabled,
            ContactsEnabled = SettingsInfo.CRMIntegrationSettingsContactsEnabled,
            ContactsTwoWaySyncEnabled = SettingsInfo.CRMIntegrationSettingsContactsTwoWaySyncEnabled,
            IgnoreExistingRecords = SettingsInfo.CRMIntegrationSettingsIgnoreExistingRecords,
            Url = SettingsInfo.CRMIntegrationSettingsUrl,
            ClientId = SettingsInfo.CRMIntegrationSettingsClientId,
            ClientSecret = SettingsInfo.CRMIntegrationSettingsClientSecret
        };

    protected abstract string CRMName { get; }
}