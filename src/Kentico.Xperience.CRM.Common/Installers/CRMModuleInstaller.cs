using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using Kentico.Xperience.CRM.Common.Classes;
using Kentico.Xperience.CRM.Common.Constants;

namespace Kentico.Xperience.CRM.Common.Installers;

/// <summary>
/// This installer creates custom module for common crm functionality
/// Currently this module contains custom class for failed synchronizations items <see cref="FailedSyncItemInfo"/>
/// and synced items class <see cref="CRMSyncItemInfo"/>
/// Custom settings are created
/// Objects are created when not exists on start.
/// </summary>
public class CRMModuleInstaller : ICRMModuleInstaller
{
    private readonly IResourceInfoProvider resourceInfoProvider;

    public CRMModuleInstaller(IResourceInfoProvider resourceInfoProvider)
    {
        this.resourceInfoProvider = resourceInfoProvider;
    }

    public void Install(string crmtype)
    {
        using (new CMSActionContext { ContinuousIntegrationAllowObjectSerialization = false })
        {
            var resourceInfo = InstallModule();
            InstallModuleClasses(resourceInfo);
            InstallSettings(resourceInfo, crmtype);
        }
    }

    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(ResourceConstants.ResourceName) ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = ResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = ResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = ResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = ResourceConstants.ResourceIsInDevelopment;
        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private void InstallModuleClasses(ResourceInfo resourceInfo)
    {
        InstallSyncedItemClass(resourceInfo);
        InstallFailedSyncItemClass(resourceInfo);
    }

    private void InstallSyncedItemClass(ResourceInfo resourceInfo)
    {
        var failedSyncItemClass = DataClassInfoProvider.GetDataClassInfo("kenticocrmcommon.crmsyncitem");
        if (failedSyncItemClass is not null)
        {
            return;
        }

        failedSyncItemClass = DataClassInfo.New("kenticocrmcommon.crmsyncitem");

        failedSyncItemClass.ClassName = "kenticocrmcommon.crmsyncitem";
        failedSyncItemClass.ClassTableName = "kenticocrmcommon.crmsyncitem".Replace(".", "_");
        failedSyncItemClass.ClassDisplayName = "CRM sync item";
        failedSyncItemClass.ClassResourceID = resourceInfo.ResourceID;
        failedSyncItemClass.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition("CRMSyncItemID");

        var formItem = new FormFieldInfo
        {
            Name = "CRMSyncItemEntityClass",
            Visible = false,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = "CRMSyncItemEntityID",
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = "CRMSyncItemCRMID",
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = "CRMSyncItemEntityCRM",
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = "CRMSyncItemCreatedByKentico", Visible = false, DataType = "boolean", Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = "CRMSyncItemLastModified",
            Visible = false,
            Precision = 0,
            DataType = "datetime",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        failedSyncItemClass.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(failedSyncItemClass);
    }

    private void InstallFailedSyncItemClass(ResourceInfo resourceInfo)
    {
        var failedSyncItemClass = DataClassInfoProvider.GetDataClassInfo(FailedSyncItemInfo.OBJECT_TYPE);
        if (failedSyncItemClass is not null)
        {
            return;
        }

        failedSyncItemClass = DataClassInfo.New(FailedSyncItemInfo.OBJECT_TYPE);

        failedSyncItemClass.ClassName = FailedSyncItemInfo.OBJECT_TYPE;
        failedSyncItemClass.ClassTableName = FailedSyncItemInfo.OBJECT_TYPE.Replace(".", "_");
        failedSyncItemClass.ClassDisplayName = "Failed sync item";
        failedSyncItemClass.ClassResourceID = resourceInfo.ResourceID;
        failedSyncItemClass.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(FailedSyncItemInfo.FailedSyncItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.FailedSyncItemEntityClass),
            Visible = false,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.FailedSyncItemEntityID),
            Visible = false,
            DataType = "integer",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.FailedSyncItemEntityCRM),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.FailedSyncItemTryCount),
            Visible = false,
            DataType = "integer",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.FailedSyncItemNextTime),
            Visible = false,
            Precision = 0,
            DataType = "datetime",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.FailedSyncItemLastModified),
            Visible = false,
            Precision = 0,
            DataType = "datetime",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        failedSyncItemClass.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(failedSyncItemClass);
    }

    private void InstallSettings(ResourceInfo resourceInfo, string crmType)
    {
        var crmIntegrations = SettingsCategoryInfo.Provider.Get("kenticocrmcommon.crmintegrations");
        if (crmIntegrations is null)
        {
            var rootSettings = SettingsCategoryInfo.Provider.Get("CMS.Settings");
            if (rootSettings is null)
                throw new InvalidOperationException("Category 'CMS.Settings' root not found");

            crmIntegrations = new SettingsCategoryInfo
            {
                CategoryName = "kenticocrmcommon.crmintegrations",
                CategoryDisplayName = "CRM integrations",
                CategoryParentID = rootSettings.CategoryID,
                CategoryLevel = 1,
                CategoryResourceID = resourceInfo.ResourceID,
                CategoryIsCustom = true,
                CategoryIsGroup = false,
                CategoryOrder = SettingsCategoryInfo.Provider.Get()
                    .Where(c => c.CategoryLevel == 1)
                    .Max(c => c.CategoryOrder) + 1
            };
            
            SettingsCategoryInfo.Provider.Set(crmIntegrations);
        }
        
        var crmCategory = SettingsCategoryInfo.Provider.Get($"kenticocrmcommon.{crmType}");
        if (crmCategory is null)
        {
            crmCategory = new SettingsCategoryInfo
            {
                CategoryName = $"kenticocrmcommon.{crmType}",
                CategoryDisplayName = $"{crmType} settings",
                CategoryParentID = crmIntegrations.CategoryID,
                CategoryLevel = 2,
                CategoryResourceID = resourceInfo.ResourceID,
                CategoryIsCustom = true,
                CategoryIsGroup = true
            };
            
            SettingsCategoryInfo.Provider.Set(crmCategory);
        }

        var settingFormsEnabled = SettingsKeyInfo.Provider.Get($"CMS{crmType}CRMIntegrationFormLeadsEnabled");
        if (settingFormsEnabled is null)
        {
            settingFormsEnabled = new SettingsKeyInfo
            {
                KeyName = $"CMS{crmType}CRMIntegrationFormLeadsEnabled",
                KeyDisplayName = "Form leads enabled",
                KeyDescription = "",
                KeyType = "boolean",
                KeyCategoryID = crmCategory.CategoryID,
                KeyIsCustom = true,
                KeyExplanationText = "",
            };
            
            SettingsKeyInfo.Provider.Set(settingFormsEnabled);
        }
        
        var settingsIgnoreExisting = SettingsKeyInfo.Provider.Get($"CMS{crmType}CRMIntegrationIgnoreExistingRecords");
        if (settingsIgnoreExisting is null)
        {
            settingsIgnoreExisting = new SettingsKeyInfo
            {
                KeyName = $"CMS{crmType}CRMIntegrationIgnoreExistingRecords",
                KeyDisplayName = "Ignore existing records",
                KeyDescription = "",
                KeyType = "boolean",
                KeyCategoryID = crmCategory.CategoryID,
                KeyIsCustom = true,
                KeyExplanationText = "If true no existing item with same email or paired record by ID is updated"
            };
            
            SettingsKeyInfo.Provider.Set(settingsIgnoreExisting);
        }
        
        var settingUrl = SettingsKeyInfo.Provider.Get($"CMS{crmType}CRMIntegration{crmType}Url");
        if (settingUrl is null)
        {
            settingUrl = new SettingsKeyInfo
            {
                KeyName = $"CMS{crmType}CRMIntegration{crmType}Url",
                KeyDisplayName = $"{crmType} URL",
                KeyDescription = "",
                KeyType = "string",
                KeyCategoryID = crmCategory.CategoryID,
                KeyIsCustom = true,
                KeyExplanationText = "",
            };
            
            SettingsKeyInfo.Provider.Set(settingUrl);
        }
        
        var settingClientId = SettingsKeyInfo.Provider.Get($"CMS{crmType}CRMIntegrationClientId");
        if (settingClientId is null)
        {
            settingClientId = new SettingsKeyInfo
            {
                KeyName = $"CMS{crmType}CRMIntegrationClientId",
                KeyDisplayName = "Client ID",
                KeyDescription = "",
                KeyType = "string",
                KeyCategoryID = crmCategory.CategoryID,
                KeyIsCustom = true,
                KeyExplanationText = "",
            };
            
            SettingsKeyInfo.Provider.Set(settingClientId);
        }
        
        var settingClientSecret = SettingsKeyInfo.Provider.Get($"CMS{crmType}CRMIntegrationClientSecret");
        if (settingClientSecret is null)
        {
            settingClientSecret = new SettingsKeyInfo
            {
                KeyName = $"CMS{crmType}CRMIntegrationClientSecret",
                KeyDisplayName = "Client Secret",
                KeyDescription = "",
                KeyType = "string",
                KeyCategoryID = crmCategory.CategoryID,
                KeyIsCustom = true,
                KeyExplanationText = "",
            };
            
            SettingsKeyInfo.Provider.Set(settingClientSecret);
        }
    }
}