using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common;

namespace Kentico.Xperience.CRM.Common.Synchronization;

public interface ICRMSyncItemService
{
    Task LogFormLeadCreateItem(BizFormItem bizFormItem, string crmId, string crmName);
    Task LogFormLeadUpdateItem(BizFormItem bizFormItem, string crmId, string crmName);
    Task<CRMSyncItemInfo?> GetFormLeadSyncItem(BizFormItem bizFormItem, string crmName);
}