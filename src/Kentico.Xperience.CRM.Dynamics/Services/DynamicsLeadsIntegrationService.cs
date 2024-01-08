using CMS.Helpers;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Services.Implementations;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;

namespace Kentico.Xperience.CRM.Dynamics.Services;

/// <summary>
/// Specific Lead integration service implementation for Dynamics Sales
/// </summary>
internal class DynamicsLeadsIntegrationService : LeadsIntegrationServiceCommon, IDynamicsLeadsIntegrationService
{
    private readonly DynamicsBizFormsMappingConfiguration bizFormMappingConfig;
    private readonly ServiceClient serviceClient;
    private readonly ILogger<DynamicsLeadsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsMonitor<DynamicsIntegrationSettings> settings;

    public DynamicsLeadsIntegrationService(
        DynamicsBizFormsMappingConfiguration bizFormMappingConfig, ILeadsIntegrationValidationService validationService,
        ServiceClient serviceClient,
        ILogger<DynamicsLeadsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsMonitor<DynamicsIntegrationSettings> settings)
        : base(bizFormMappingConfig, validationService, logger)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;
        this.serviceClient = serviceClient;
        this.logger = logger;
        this.syncItemService = syncItemService;
        this.failedSyncItemService = failedSyncItemService;
        this.settings = settings;
    }

    protected override async Task SynchronizeLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        try
        {
            var syncItem = syncItemService.GetFormLeadSyncItem(bizFormItem, CRMType.Dynamics);
            
            if (syncItem is null)
            {
                await UpdateByEmailOrCreate(bizFormItem, fieldMappings);
            }
            else
            {
                var existingLead = await GetLeadById(Guid.Parse(syncItem.CRMSyncItemCRMID));
                if (existingLead is null)
                {
                    await UpdateByEmailOrCreate(bizFormItem, fieldMappings);
                }
                else if (!settings.CurrentValue.IgnoreExistingRecords)
                {
                    await UpdateLeadAsync(existingLead, bizFormItem, fieldMappings);
                }
            }
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Sync lead failed - api error: {ApiResult}", e.Detail);
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.Dynamics);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Sync lead failed - api error: {ApiResult}", ie.Detail);
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.Dynamics);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Sync lead failed - unknown api error");
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.Dynamics);
        }
    }

    private async Task UpdateByEmailOrCreate(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        Lead? existingLead = null;
        var tmpLead = new Lead();
        MapLead(bizFormItem, tmpLead, fieldMappings);
        
        if (!string.IsNullOrWhiteSpace(tmpLead.EMailAddress1))
        {
            existingLead = await GetLeadByEmail(tmpLead.EMailAddress1);
        }

        if (existingLead is null)
        {
            await CreateLeadAsync(bizFormItem, fieldMappings);
        }
        else if (!settings.CurrentValue.IgnoreExistingRecords)
        {
            await UpdateLeadAsync(existingLead, bizFormItem, fieldMappings);
        }
    }

    private async Task CreateLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        var leadEntity = new Lead();
        MapLead(bizFormItem, leadEntity, fieldMappings);

        if (leadEntity.Subject is null)
        {
            leadEntity.Subject = $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";
        }

        var leadId = await serviceClient.CreateAsync(leadEntity);
        
        syncItemService.LogFormLeadCreateItem(bizFormItem, leadId.ToString(), CRMType.Dynamics);  
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    private async Task UpdateLeadAsync(Lead leadEntity, BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        MapLead(bizFormItem, leadEntity, fieldMappings);

        if (leadEntity.Subject is null)
        {
            leadEntity.Subject = $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";
        }

        await serviceClient.UpdateAsync(leadEntity);
        
        syncItemService.LogFormLeadUpdateItem(bizFormItem, leadEntity.LeadId.ToString()!, CRMType.Dynamics);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    protected virtual void MapLead(BizFormItem bizFormItem, Lead leadEntity,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.FormFieldMapping.MapFormField(bizFormItem);

            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => leadEntity[m.CrmFieldName] = formFieldValue,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }

    private async Task<Lead?> GetLeadById(Guid leadId)
        => (await serviceClient.RetrieveAsync(Lead.EntityLogicalName, leadId, new ColumnSet(true)))?.ToEntity<Lead>();

    private async Task<Lead?> GetLeadByEmail(string email)
    {
        var query = new QueryExpression(Lead.EntityLogicalName) { ColumnSet = new ColumnSet(true), TopCount = 1 };
        query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);

        return (await serviceClient.RetrieveMultipleAsync(query)).Entities.FirstOrDefault()?.ToEntity<Lead>();
    }
}