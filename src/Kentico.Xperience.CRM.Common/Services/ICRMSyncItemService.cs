using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Classes;

namespace Kentico.Xperience.CRM.Common.Services;

public interface ICRMSyncItemService
{
    void LogFormLeadCreateItem(BizFormItem bizFormItem, string crmId, string crmName);
    void LogFormLeadUpdateItem(BizFormItem bizFormItem, string crmId, string crmName);
    CRMSyncItemInfo? GetFormLeadSyncItem(BizFormItem bizFormItem, string crmName);
}