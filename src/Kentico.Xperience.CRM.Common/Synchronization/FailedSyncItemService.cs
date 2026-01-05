using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Synchronization;

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
        => LogFailedCRMItem(crmName, bizFormItem.BizFormClassName, bizFormItem.ItemID);

    public void LogFailedContactItem(ContactInfo contactInfo, string crmName)
        => LogFailedCRMItem(crmName, contactInfo.ClassName, contactInfo.ContactID);

    public void LogFailedLeadItems(IEnumerable<BizFormItem> bizFormItems, string crmName)
    {
        foreach (var item in bizFormItems)
        {
            LogFailedLeadItem(item, crmName);
        }
    }

    public void LogFailedContactItems(IEnumerable<ContactInfo> contactInfos, string crmName)
    {
        foreach (var contactInfo in contactInfos)
        {
            LogFailedContactItem(contactInfo, crmName);
        }
    }

    private void LogFailedCRMItem(string crmName, string entityClass, int entityId)
    {
        var existingItem = GetExistingItem(crmName, entityClass, entityId);

        if (existingItem is null)
        {
            existingItem = new FailedSyncItemInfo
            {
                FailedSyncItemEntityClass = entityClass,
                FailedSyncItemEntityID = entityId,
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

    public void DeleteFailedSyncItem(string crmName, string entityClass, int entityId)
    {
        GetExistingItem(crmName, entityClass, entityId)?.Delete();
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