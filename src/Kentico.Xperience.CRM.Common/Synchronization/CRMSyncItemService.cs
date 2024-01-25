using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common;

namespace Kentico.Xperience.CRM.Common.Synchronization;

internal class CRMSyncItemService : ICRMSyncItemService
{
    private readonly ICRMSyncItemInfoProvider crmSyncItemInfoProvider;

    public CRMSyncItemService(ICRMSyncItemInfoProvider crmSyncItemInfoProvider)
    {
        this.crmSyncItemInfoProvider = crmSyncItemInfoProvider;
    }

    public async Task LogFormLeadCreateItem(BizFormItem bizFormItem, string crmId, string crmName)
        => await LogFormLeadSyncItem(bizFormItem, crmId, crmName, true);

    public async Task LogFormLeadUpdateItem(BizFormItem bizFormItem, string crmId, string crmName)
        => await LogFormLeadSyncItem(bizFormItem, crmId, crmName, false);

    private async Task LogFormLeadSyncItem(BizFormItem bizFormItem, string crmId, string crmName, bool createdByKentico)
    {
        var syncItem = await GetFormLeadSyncItem(bizFormItem, crmName);
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

    public async Task<CRMSyncItemInfo?> GetFormLeadSyncItem(BizFormItem bizFormItem, string crmName)
        => (await crmSyncItemInfoProvider.Get()
            .TopN(1)
            .WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityClass), bizFormItem.BizFormClassName)
            .WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityID), bizFormItem.ItemID)
            .WhereEquals(nameof(CRMSyncItemInfo.CRMSyncItemEntityCRM), crmName)
            .GetEnumerableTypedResultAsync())
            .FirstOrDefault();

}