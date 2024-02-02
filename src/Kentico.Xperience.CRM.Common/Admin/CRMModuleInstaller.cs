using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace Kentico.Xperience.CRM.Common.Admin;

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

    public void Install(string crmType)
    {
        var resourceInfo = InstallModule();
        InstallModuleClasses(resourceInfo);
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
        InstallCRMIntegrationSettingsClass(resourceInfo);
        InstallContactsLastSyncTimeClass(resourceInfo);
    }

    private void InstallSyncedItemClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(CRMSyncItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(CRMSyncItemInfo.OBJECT_TYPE);

        info.ClassName = CRMSyncItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = CRMSyncItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "CRM Successful sync item";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

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
            Name = nameof(CRMSyncItemInfo.CRMSyncItemCreatedByKentico),
            Visible = false,
            DataType = "boolean",
            Enabled = true
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

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    private void InstallFailedSyncItemClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(FailedSyncItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(FailedSyncItemInfo.OBJECT_TYPE);

        info.ClassName = FailedSyncItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = FailedSyncItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "CRM Failed sync item";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

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

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    private void InstallCRMIntegrationSettingsClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(CRMIntegrationSettingsInfo.OBJECT_TYPE) ?? DataClassInfo.New(CRMIntegrationSettingsInfo.OBJECT_TYPE);

        info.ClassName = CRMIntegrationSettingsInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = CRMIntegrationSettingsInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "CRM integration settings";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo =
            FormHelper.GetBasicFormDefinition(nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsFormsEnabled),
            Caption = "Forms enabled",
            Visible = true,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsContactsEnabled),
            Caption = "Contacts enabled",
            Visible = false,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsContactsTwoWaySyncEnabled),
            Caption = "Contacts two way sync enabled",
            DefaultValue = "True",
            Visible = false,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsIgnoreExistingRecords),
            Caption = "Ignore existing records",
            Visible = true,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsUrl),
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
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsClientId),
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
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsClientSecret),
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
            Name = nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsCRMName),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }

    private void InstallContactsLastSyncTimeClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ContactsLastSyncInfo.OBJECT_TYPE) ?? DataClassInfo.New(ContactsLastSyncInfo.OBJECT_TYPE);

        info.ClassName = ContactsLastSyncInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ContactsLastSyncInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "CRM Contacts last sync";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo =
            FormHelper.GetBasicFormDefinition(nameof(ContactsLastSyncInfo.ContactsLastSyncItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(ContactsLastSyncInfo.ContactsLastSyncCRM),
            Visible = true,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ContactsLastSyncInfo.ContactsLastSyncTime),
            Visible = true,
            Precision = 3,
            DataType = "datetime",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }
}