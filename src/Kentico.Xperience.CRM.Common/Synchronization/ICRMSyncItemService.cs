using CMS.ContactManagement;
using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Synchronization;

public interface ICRMSyncItemService
{
    Task LogFormLeadCreateItem(BizFormItem bizFormItem, string crmId, string crmName);
    Task LogFormLeadUpdateItem(BizFormItem bizFormItem, string crmId, string crmName);
    Task<CRMSyncItemInfo?> GetFormLeadSyncItem(BizFormItem bizFormItem, string crmName);
    
    Task LogContactSyncItem(ContactInfo contactInfo, string crmId, string crmName);
    Task<CRMSyncItemInfo?> GetContactSyncItem(ContactInfo contactInfo, string crmName);
}