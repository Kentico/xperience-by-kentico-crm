﻿using CMS.ContactManagement;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Classes;

namespace Kentico.Xperience.CRM.Common.Services;

public interface ICRMSyncItemService
{
    Task LogFormLeadCreateItem(BizFormItem bizFormItem, string crmId, string crmName);
    Task LogFormLeadUpdateItem(BizFormItem bizFormItem, string crmId, string crmName);
    Task<CRMSyncItemInfo?> GetFormLeadSyncItem(BizFormItem bizFormItem, string crmName);
    
    Task LogContactCreateItem(ContactInfo contactInfo, string crmId, string crmName);
    Task LogContactUpdateItem(ContactInfo contactInfo, string crmId, string crmName);
    Task<CRMSyncItemInfo?> GetContactSyncItem(ContactInfo contactInfo, string crmName);
}