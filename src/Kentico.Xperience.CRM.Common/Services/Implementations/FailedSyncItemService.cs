using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Classes;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

internal class FailedSyncItemService : IFailedSyncItemService
{
    private readonly IFailedSyncItemInfoProvider failedSyncItemInfoProvider;
    private readonly IBizFormInfoProvider bizFormInfoProvider;

    public FailedSyncItemService(IFailedSyncItemInfoProvider failedSyncItemInfoProvider,
        IBizFormInfoProvider bizFormInfoProvider
    )
    {
        this.failedSyncItemInfoProvider = failedSyncItemInfoProvider;
        this.bizFormInfoProvider = bizFormInfoProvider;
    }

    public void LogFailedLeadItem(BizFormItem bizFormItem, string crmName)
    {
        var existingItem = failedSyncItemInfoProvider.Get()
            .WhereEquals(nameof(FailedSyncItemInfo.ClassName), bizFormItem.BizFormClassName)
            .WhereEquals(nameof(FailedSyncItemInfo.EntityID), bizFormItem.ItemID)
            .WhereEquals(nameof(FailedSyncItemInfo.EntityCRM), crmName)
            .TopN(1)
            .FirstOrDefault();

        if (existingItem is null)
        {
            existingItem = new FailedSyncItemInfo
            {
                EntityClass = bizFormItem.BizFormClassName,
                EntityID = bizFormItem.ItemID,
                SyncNextTime = DateTime.Now.AddMinutes(1)
            };
        }
        else
        {
            existingItem.SyncTryCount++;
            // next times for re-sync in 2,4,8,16,32,64... minutes
            existingItem.SyncNextTime = DateTime.Now.AddMinutes(Math.Pow(2, existingItem.SyncTryCount));
        }

        failedSyncItemInfoProvider.Set(existingItem);
    }

    public IEnumerable<FailedSyncItemInfo> GetFailedSyncItemsToReSync(string crmName)
    {
        return failedSyncItemInfoProvider.Get()
            .WhereEquals(nameof(FailedSyncItemInfo.EntityCRM), crmName)
            .WhereGreaterOrEquals(nameof(FailedSyncItemInfo.SyncNextTime), DateTime.Now);
    }

    public BizFormItem GetBizFormItem(FailedSyncItemInfo failedSyncItemInfo)
    {
        DataClassInfo formClass = DataClassInfoProvider.GetDataClassInfo(failedSyncItemInfo.EntityClass);
        BizFormItemProvider.GetItems(failedSyncItemInfo.EntityClass);
        //@TODO
        return null!;
    }
}