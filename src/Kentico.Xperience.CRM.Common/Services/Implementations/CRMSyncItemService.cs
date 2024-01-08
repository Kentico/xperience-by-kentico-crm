using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Classes;
using Kentico.Xperience.CRM.Common.Constants;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

internal class CRMSyncItemService : ICRMSyncItemService
{
    private readonly ICRMSyncItemInfoProvider crmSyncItemInfoProvider;

    public CRMSyncItemService(ICRMSyncItemInfoProvider crmSyncItemInfoProvider)
    {
        this.crmSyncItemInfoProvider = crmSyncItemInfoProvider;
    }

    public void LogFormLeadCreateItem(BizFormItem bizFormItem, string crmId, string crmName)
        => LogFormLeadSyncItem(bizFormItem, crmId, crmName, false);

    public void LogFormLeadUpdateItem(BizFormItem bizFormItem, string crmId, string crmName)
        => LogFormLeadSyncItem(bizFormItem, crmId, crmName, false);

    private void LogFormLeadSyncItem(BizFormItem bizFormItem, string crmId, string crmName, bool createdByKentico)
    {
        var syncItem = GetFormLeadSyncItem(bizFormItem, crmName);
        if (syncItem is null)
        {
            new CRMSyncItemInfo
            {
                CRMSyncItemEntityID = bizFormItem.ItemID.ToString(),
                CRMSyncItemEntityClass = bizFormItem.BizFormClassName,
                CRMSyncItemEntityCRM = crmName,
                CRMSyncItemCRMID = crmId,
                CRMSyncItemCreatedByKentico = createdByKentico
            }.Insert();
        }
        else
        {
            syncItem.CRMSyncItemCRMID = crmId;
            syncItem.CRMSyncItemCreatedByKentico = createdByKentico;
            syncItem.Update();
        }
    }

    public CRMSyncItemInfo? GetFormLeadSyncItem(BizFormItem bizFormItem, string crmName)
        => crmSyncItemInfoProvider.Get()
            .TopN(1)
            .WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityClass), bizFormItem.BizFormClassName)
            .WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityID), bizFormItem.ItemID)
            .WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityCRM), crmName)
            .FirstOrDefault();

}