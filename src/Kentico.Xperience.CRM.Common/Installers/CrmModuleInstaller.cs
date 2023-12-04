using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using Kentico.Xperience.CRM.Common.Classes;
using Kentico.Xperience.CRM.Common.Constants;

namespace Kentico.Xperience.CRM.Common.Installers;

public class CrmModuleInstaller : ICrmModuleInstaller
{
    private readonly IResourceInfoProvider resourceInfoProvider;

    public CrmModuleInstaller(IResourceInfoProvider resourceInfoProvider)
    {
        this.resourceInfoProvider = resourceInfoProvider;
    }

    public void Install()
    {
        // using (new CMSActionContext { ContinuousIntegrationAllowObjectSerialization = false })
        // {
        //     var resourceInfo = InstallModule();
        //     InstallModuleClasses(resourceInfo);
        // }
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
        InstallFailedSyncItemClass(resourceInfo);
    }

    private void InstallFailedSyncItemClass(ResourceInfo resourceInfo)
    {
        var failedSyncItemClass = DataClassInfoProvider.GetDataClassInfo(FailedSyncItemInfo.OBJECT_TYPE) ??
                                  DataClassInfo.New(FailedSyncItemInfo.OBJECT_TYPE);

        failedSyncItemClass.ClassName = FailedSyncItemInfo.OBJECT_TYPE;
        failedSyncItemClass.ClassTableName = FailedSyncItemInfo.OBJECT_TYPE.Replace(".", "_");
        failedSyncItemClass.ClassDisplayName = "Failed sync item";
        failedSyncItemClass.ClassResourceID = resourceInfo.ResourceID;
        failedSyncItemClass.ClassType = ClassType.OTHER;
        
        var formInfo = FormHelper.GetBasicFormDefinition(nameof(FailedSyncItemInfo.FailedSyncItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.EntityType),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.EntityClass),
            Visible = false,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.EntityID), 
            Visible = false, 
            DataType = "integer", 
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.EntityCRM),
            Visible = false,
            Precision = 0,
            Size = 50,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.SyncTryCount),
            Visible = false, 
            DataType = "integer", 
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.SyncNextTime),
            Visible = false,
            Precision = 0,
            DataType = "datetime",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FailedSyncItemInfo.SyncLastModified),
            Visible = false,
            Precision = 0,
            DataType = "datetime",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        failedSyncItemClass.ClassFormDefinition = formInfo.GetXmlDefinition();

        if (failedSyncItemClass.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(failedSyncItemClass);
        }
    }
}