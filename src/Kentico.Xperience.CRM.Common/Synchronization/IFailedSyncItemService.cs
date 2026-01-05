using CMS.ContactManagement;
using CMS.OnlineForms;


namespace Kentico.Xperience.CRM.Common.Synchronization;

/// <summary>
/// Service for management failed synchronization items (currently leads/biz-form items)
/// </summary>
public interface IFailedSyncItemService
{
    /// <summary>
    /// Creates new record in failed items table or increment TrySyncCount property when record exists.
    /// Next sync time is planned.
    /// </summary>
    /// <param name="bizFormItem">BizForm item</param>
    /// <param name="crmName">CRM name</param>
    void LogFailedLeadItem(BizFormItem bizFormItem, string crmName);

    /// <summary>
    /// Creates new record in failed items table or increment TrySyncCount property when record exists.
    /// Next sync time is planned.
    /// </summary>
    /// <param name="contactInfo"></param>
    /// <param name="crmName"></param>
    void LogFailedContactItem(ContactInfo contactInfo, string crmName);

    /// <summary>
    /// Creates new records in failed items table or increment TrySyncCount property when record exists.
    /// Next sync time is planned.
    /// </summary>
    /// <param name="bizFormItems"></param>
    /// <param name="crmName">CRM name</param>
    void LogFailedLeadItems(IEnumerable<BizFormItem> bizFormItems, string crmName);

    /// <summary>
    /// Creates new records in failed items table or increment TrySyncCount property when record exists.
    /// Next sync time is planned.
    /// </summary>
    /// <param name="contactInfos"></param>
    /// <param name="crmName"></param>
    void LogFailedContactItems(IEnumerable<ContactInfo> contactInfos, string crmName);

    /// <summary>
    /// Get all items waiting for synchronization which can be already synced again (according SyncNextTime property) 
    /// </summary>
    /// <param name="crmName">CRM name</param>
    /// <returns></returns>
    IEnumerable<FailedSyncItemInfo> GetFailedSyncItemsToReSync(string crmName);

    /// <summary>
    /// Load BizForm item data for given failed sync item
    /// </summary>
    /// <param name="failedSyncItemInfo"></param>
    /// <returns></returns>
    BizFormItem? GetBizFormItem(FailedSyncItemInfo failedSyncItemInfo);

    /// <summary>
    /// Delete record for given CRM, class name and ID
    /// </summary>
    /// <param name="crmName">CRM name</param>
    /// <param name="entityClass">Entity class</param>
    /// <param name="entityId">Entity ID</param>
    void DeleteFailedSyncItem(string crmName, string entityClass, int entityId);
}