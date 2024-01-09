using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Services.Implementations;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesForce.OpenApi;
using System.Text.Json;

namespace Kentico.Xperience.CRM.SalesForce.Services;

internal class SalesForceLeadsIntegrationService : LeadsIntegrationServiceCommon, ISalesForceLeadsIntegrationService
{
    private readonly SalesForceBizFormsMappingConfiguration bizFormMappingConfig;
    private readonly ISalesForceApiService apiService;
    private readonly ILogger<SalesForceLeadsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsMonitor<SalesForceIntegrationSettings> settings;

    public SalesForceLeadsIntegrationService(
        SalesForceBizFormsMappingConfiguration bizFormMappingConfig,
        ILeadsIntegrationValidationService validationService,
        ISalesForceApiService apiService,
        ILogger<SalesForceLeadsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsMonitor<SalesForceIntegrationSettings> settings)
        : base(bizFormMappingConfig, validationService, logger)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;
        this.apiService = apiService;
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
            var syncItem = syncItemService.GetFormLeadSyncItem(bizFormItem, CRMType.SalesForce);

            if (syncItem is null)
            {
                await UpdateByEmailOrCreate(bizFormItem, fieldMappings);
            }
            else
            {
                var existingLead = await apiService.GetLeadById(syncItem.CRMSyncItemCRMID);
                if (existingLead is null)
                {
                    await UpdateByEmailOrCreate(bizFormItem, fieldMappings);
                }
                else if (!settings.CurrentValue.IgnoreExistingRecords)
                {
                    await UpdateLeadAsync(existingLead.Id!, bizFormItem, fieldMappings);
                }
            }
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Update lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.SalesForce);
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Update lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.SalesForce);
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Update lead failed - unexpected api error");
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.SalesForce);
        }
    }

    private async Task UpdateByEmailOrCreate(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        string? existingLeadId = null;
        
        var tmpLead = new LeadSObject();
        MapLead(bizFormItem, tmpLead, fieldMappings);
        
        if (!string.IsNullOrWhiteSpace(tmpLead.Email))
        {
            existingLeadId = await apiService.GetLeadByEmail(tmpLead.Email);
        }

        if (existingLeadId is null)
        {
            await CreateLeadAsync(bizFormItem, fieldMappings);
        }
        else if (!settings.CurrentValue.IgnoreExistingRecords)
        {
            await UpdateLeadAsync(existingLeadId, bizFormItem, fieldMappings);
        }
    }

    private async Task CreateLeadAsync(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        var lead = new LeadSObject();
        MapLead(bizFormItem, lead, fieldMappings);

        lead.LeadSource ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";
        lead.Company ??= "undefined"; //required field - set to 'undefined' to prevent errors

        var result = await apiService.CreateLeadAsync(lead);

        syncItemService.LogFormLeadCreateItem(bizFormItem, result.Id!, CRMType.SalesForce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.SalesForce, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    private async Task UpdateLeadAsync(string leadId, BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        var lead = new LeadSObject();
        MapLead(bizFormItem, lead, fieldMappings);
        
        lead.LeadSource ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";

        await apiService.UpdateLeadAsync(leadId, lead);
        
        syncItemService.LogFormLeadUpdateItem(bizFormItem, leadId, CRMType.SalesForce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.SalesForce, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    protected virtual void MapLead(BizFormItem bizFormItem, LeadSObject lead,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.FormFieldMapping.MapFormField(bizFormItem);
            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => lead.AdditionalProperties[m.CrmFieldName] = formFieldValue,
                CRMFieldMappingFunction<LeadSObject> m => m.MapCrmField(lead, formFieldValue),
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }
}