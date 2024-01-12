using CMS.DataEngine;
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
                FailedSyncItemEntityClass = bizFormItem.BizFormClassName,
                FailedSyncItemEntityID = bizFormItem.ItemID,
                FailedSyncItemEntityCRM = crmName,
                FailedSyncItemNextTime = DateTime.Now.AddMinutes(1),
                FailedSyncItemTryCount = 0
            };
        }
        else if (existingItem.FailedSyncItemTryCount < MaxSyncCount)
        {
            existingItem.FailedSyncItemTryCount++;
            // next times for re-sync in 2,4,8,16,32,64... minutes
            existingItem.FailedSyncItemNextTime = DateTime.Now.AddMinutes(Math.Pow(2, existingItem.FailedSyncItemTryCount));
        }
        else
        {
            existingItem.SetValue(nameof(FailedSyncItemInfo.FailedSyncItemNextTime), null);
        }

        failedSyncItemInfoProvider.Set(existingItem);
    }

    public IEnumerable<FailedSyncItemInfo> GetFailedSyncItemsToReSync(string crmName)
    {
        return failedSyncItemInfoProvider.Get()
            .WhereEquals(nameof(FailedSyncItemInfo.FailedSyncItemEntityCRM), crmName)
            .WhereLessOrEquals(nameof(FailedSyncItemInfo.FailedSyncItemNextTime), DateTime.Now)
            .OrderBy(nameof(FailedSyncItemInfo.FailedSyncItemEntityID));
    }

    public BizFormItem? GetBizFormItem(FailedSyncItemInfo failedSyncItemInfo)
    {
        var formClass = DataClassInfoProvider.GetDataClassInfo(failedSyncItemInfo.FailedSyncItemEntityClass);
        if (formClass is null)
        {
            return null;
        }

        var primaryColumnName = new BizFormItem(formClass.ClassName).TypeInfo.IDColumn;

        return BizFormItemProvider.GetItems(failedSyncItemInfo.FailedSyncItemEntityClass)
            .WhereEquals(primaryColumnName, failedSyncItemInfo.FailedSyncItemEntityID)
            .FirstOrDefault();
    }

    public void DeleteFailedSyncItem(string crmCrmName, string entityClass, int entityId)
    {
        GetExistingItem(crmCrmName, entityClass, entityId)?.Delete();
    }

    private FailedSyncItemInfo? GetExistingItem(string crmName, string entityClass, int entityId)
    {
        return failedSyncItemInfoProvider.Get()
            .WhereEquals(nameof(FailedSyncItemInfo.FailedSyncItemEntityClass), entityClass)
            .WhereEquals(nameof(FailedSyncItemInfo.FailedSyncItemEntityID), entityId)
            .WhereEquals(nameof(FailedSyncItemInfo.FailedSyncItemEntityCRM), crmName)
            .TopN(1)
            .FirstOrDefault();
    }
}