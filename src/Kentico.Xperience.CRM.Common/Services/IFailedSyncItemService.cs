using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Classes;

namespace Kentico.Xperience.CRM.Common.Services;

public interface IFailedSyncItemService
{
    void LogFailedLeadItem(BizFormItem bizFormItem, string crmName);
    IEnumerable<FailedSyncItemInfo> GetFailedSyncItemsToReSync(string crmName);
    BizFormItem? GetBizFormItem(FailedSyncItemInfo failedSyncItemInfo);
    void DeleteFailedSyncItem(string crmCrmName, string entityClass, int entityId);
}