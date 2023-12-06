using CMS.DataEngine;
using CMS.FormEngine;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Classes;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

internal class FailedSyncItemService : IFailedSyncItemService
{
    private readonly IFailedSyncItemInfoProvider failedSyncItemInfoProvider;

    /// <summary>
    /// Preventing infinite syncing
    /// </summary>
    private const int MaxSyncCount = 10;
    
    public FailedSyncItemService(IFailedSyncItemInfoProvider failedSyncItemInfoProvider
    )
    {
        this.failedSyncItemInfoProvider = failedSyncItemInfoProvider;
    }

    public void LogFailedLeadItem(BizFormItem bizFormItem, string crmName)
    {
        var existingItem = GetExistingItem(crmName, bizFormItem.BizFormClassName, bizFormItem.ItemID);
        
        if (existingItem is null)
        {
            existingItem = new FailedSyncItemInfo
            {
                EntityClass = bizFormItem.BizFormClassName,
                EntityID = bizFormItem.ItemID,
                EntityCRM = crmName,
                SyncNextTime = DateTime.Now.AddMinutes(1),
                SyncTryCount = 0
            };
        }
        else if (existingItem.SyncTryCount < MaxSyncCount)
        {
            existingItem.SyncTryCount++;
            // next times for re-sync in 2,4,8,16,32,64... minutes
            existingItem.SyncNextTime = DateTime.Now.AddMinutes(Math.Pow(2, existingItem.SyncTryCount));
        }
        else
        {
            existingItem.SetValue(nameof(FailedSyncItemInfo.SyncNextTime), null);
        }

        failedSyncItemInfoProvider.Set(existingItem);
    }

    public IEnumerable<FailedSyncItemInfo> GetFailedSyncItemsToReSync(string crmName)
    {
        return failedSyncItemInfoProvider.Get()
            .WhereEquals(nameof(FailedSyncItemInfo.EntityCRM), crmName)
            .WhereLessOrEquals(nameof(FailedSyncItemInfo.SyncNextTime), DateTime.Now)
            .OrderBy(nameof(FailedSyncItemInfo.EntityID));
    }

    public BizFormItem? GetBizFormItem(FailedSyncItemInfo failedSyncItemInfo)
    {
        var formClass = DataClassInfoProvider.GetDataClassInfo(failedSyncItemInfo.EntityClass);
        if (formClass is null)
        {
            return null;
        }
        
        var primaryColumnName = new BizFormItem(formClass.ClassName).TypeInfo.IDColumn;

        return BizFormItemProvider.GetItems(failedSyncItemInfo.EntityClass)
            .WhereEquals(primaryColumnName, failedSyncItemInfo.EntityID)
            .FirstOrDefault();
    }

    public void DeleteFailedSyncItem(string crmCrmName, string entityClass, int entityId)
    {
        GetExistingItem(crmCrmName, entityClass, entityId)?.Delete();
    }

    private FailedSyncItemInfo? GetExistingItem(string crmName, string entityClass, int entityId)
    {
        return failedSyncItemInfoProvider.Get()
            .WhereEquals(nameof(FailedSyncItemInfo.EntityClass), entityClass)
            .WhereEquals(nameof(FailedSyncItemInfo.EntityID), entityId)
            .WhereEquals(nameof(FailedSyncItemInfo.EntityCRM), crmName)
            .TopN(1)
            .FirstOrDefault();
    }
}