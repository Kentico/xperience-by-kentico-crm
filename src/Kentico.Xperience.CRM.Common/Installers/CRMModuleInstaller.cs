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
internal class CRMModuleInstaller : ICRMModuleInstaller
{
    private readonly IResourceInfoProvider resourceInfoProvider;

    public CRMModuleInstaller(IResourceInfoProvider resourceInfoProvider)
    {
        this.resourceInfoProvider = resourceInfoProvider;
    }

    public void Install(string crmtype)
    {
        var resourceInfo = InstallModule();
        InstallModuleClasses(resourceInfo);
        InstallCRMIntegrationSettingsClass(resourceInfo);
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
        var failedSyncItemClass = DataClassInfoProvider.GetDataClassInfo(CRMSyncItemInfo.OBJECT_TYPE);
        if (failedSyncItemClass is not null)
        {
            return;
        }

        failedSyncItemClass = DataClassInfo.New(CRMSyncItemInfo.OBJECT_TYPE);

        failedSyncItemClass.ClassName = CRMSyncItemInfo.TYPEINFO.ObjectClassName;
        failedSyncItemClass.ClassTableName = CRMSyncItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        failedSyncItemClass.ClassDisplayName = "CRM sync item";
        failedSyncItemClass.ClassResourceID = resourceInfo.ResourceID;
        failedSyncItemClass.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(CRMSyncItemInfo.CRMSyncItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(CRMSyncItemInfo.CRMSyncItemEntityClass),
            Visible = false,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMSyncItemInfo.CRMSyncItemEntityID),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMSyncItemInfo.CRMSyncItemCRMID),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMSyncItemInfo.CRMSyncItemEntityCRM),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMSyncItemInfo.CRMSyncItemCreatedByKentico), Visible = false, DataType = "boolean", Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMSyncItemInfo.CRMSyncItemLastModified),
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

        failedSyncItemClass.ClassName = FailedSyncItemInfo.TYPEINFO.ObjectClassName;
        failedSyncItemClass.ClassTableName = FailedSyncItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
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
    
    private void InstallCRMIntegrationSettingsClass(ResourceInfo resourceInfo)
    {
        var settingsCRM = DataClassInfoProvider.GetDataClassInfo("kenticocrmcommon.crmintegrationsettings");
        if (settingsCRM is not null)
        {
            return;
        }

        settingsCRM = DataClassInfo.New("kenticocrmcommon.crmintegrationsettings");

        settingsCRM.ClassName = "KenticoCRMCommon.CRMIntegrationSettings";
        settingsCRM.ClassTableName = "KenticoCRMCommon_CRMIntegrationSettings";
        settingsCRM.ClassDisplayName = "CRM integration settings";
        settingsCRM.ClassResourceID = resourceInfo.ResourceID;
        settingsCRM.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition("CRMIntegrationSettingsItemID");
        
        var formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsFormsEnabled",
            Caption = "Forms enabled",
            Visible = true,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);
        
        formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsContactsEnabled",
            Caption = "Contacts enabled",
            Visible = false,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);
        
        formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsIgnoreExistingRecords",
            Caption = "Ignore existing records",
            Visible = true,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsUrl",
            Caption = "CRM URL",
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);
        
        formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsClientId",
            Caption = "Client ID",
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);
        
        formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsClientSecret",
            Caption = "Client Secret",
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);
        
        formItem = new FormFieldInfo
        {
            Name = "CRMIntegrationSettingsCRMType",
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);
        

        settingsCRM.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(settingsCRM);
    }
}