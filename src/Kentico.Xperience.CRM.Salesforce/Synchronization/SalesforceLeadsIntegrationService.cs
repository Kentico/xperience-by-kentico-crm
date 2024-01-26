using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salesforce.OpenApi;
using System.Text.Json;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

internal class SalesforceLeadsIntegrationService : ISalesforceLeadsIntegrationService
{
    private readonly SalesforceBizFormsMappingConfiguration bizFormMappingConfig;
    private readonly ILeadsIntegrationValidationService validationService;
    private readonly ISalesforceApiService apiService;
    private readonly ILogger<SalesforceLeadsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsSnapshot<SalesforceIntegrationSettings> settings;
    private readonly IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> formsConverters;

    public SalesforceLeadsIntegrationService(
        SalesforceBizFormsMappingConfiguration bizFormMappingConfig,
        ILeadsIntegrationValidationService validationService,
        ISalesforceApiService apiService,
        ILogger<SalesforceLeadsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsSnapshot<SalesforceIntegrationSettings> settings,
        IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> formsConverters)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;
        this.validationService = validationService;
        this.apiService = apiService;
        this.logger = logger;
        this.syncItemService = syncItemService;
        this.failedSyncItemService = failedSyncItemService;
        this.settings = settings;
        this.formsConverters = formsConverters;
    }

    /// <summary>
    /// Validates BizForm item, then get specific mapping and finally specific implementation is called
    /// from inherited service
    /// </summary>
    /// <param name="bizFormItem"></param>
    public async Task SynchronizeLeadAsync(BizFormItem bizFormItem)
    {
        var leadConverters = Enumerable.Empty<ICRMTypeConverter<BizFormItem, LeadSObject>>();
        var leadMapping = Enumerable.Empty<BizFormFieldMapping>();

        if (bizFormMappingConfig.FormsConverters.TryGetValue(bizFormItem.BizFormClassName.ToLowerInvariant(),
                out var formConverters))
        {
            leadConverters = formsConverters.Where(c => formConverters.Contains(c.GetType()));
        }

        if (bizFormMappingConfig.FormsMappings.TryGetValue(bizFormItem.BizFormClassName.ToLowerInvariant(),
                out var formMapping))
        {
            leadMapping = formMapping;
        }

        if (leadConverters.Any() || leadMapping.Any())
        {
            if (!await validationService.ValidateFormItem(bizFormItem))
            {
                logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} refused by validation",
                    bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
                return;
            }

            await SynchronizeLeadAsync(bizFormItem, leadMapping, leadConverters);
        }
    }

    private async Task SynchronizeLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings,
        IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> converters)
    {
        try
        {
            var syncItem = await syncItemService.GetFormLeadSyncItem(bizFormItem, CRMType.Salesforce);

            if (syncItem is null)
            {
                await UpdateByEmailOrCreate(bizFormItem, fieldMappings, converters);
            }
            else
            {
                var existingLead = await apiService.GetLeadById(syncItem.CRMSyncItemCRMID, nameof(LeadSObject.Id));
                if (existingLead is null)
                {
                    await UpdateByEmailOrCreate(bizFormItem, fieldMappings, converters);
                }
                else if (!settings.Value.IgnoreExistingRecords)
                {
                    await UpdateLeadAsync(existingLead.Id!, bizFormItem, fieldMappings, converters);
                }
                else
                {
                    logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} ignored",
                        bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
                }
            }
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Sync lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.Salesforce);
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Sync lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.Salesforce);
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Sync lead failed - unexpected api error");
            failedSyncItemService.LogFailedLeadItem(bizFormItem, CRMType.Salesforce);
        }
    }

    private async Task UpdateByEmailOrCreate(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings,
        IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> converters)
    {
        string? existingLeadId = null;

        var tmpLead = new LeadSObject();
        await MapLead(bizFormItem, tmpLead, fieldMappings, converters);

        if (!string.IsNullOrWhiteSpace(tmpLead.Email))
        {
            existingLeadId = await apiService.GetLeadByEmail(tmpLead.Email);
        }

        if (existingLeadId is null)
        {
            await CreateLeadAsync(bizFormItem, fieldMappings, converters);
        }
        else if (!settings.Value.IgnoreExistingRecords)
        {
            await UpdateLeadAsync(existingLeadId, bizFormItem, fieldMappings, converters);
        }
        else
        {
            logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} ignored",
                bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
        }
    }

    private async Task CreateLeadAsync(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings,
        IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> converters)
    {
        var lead = new LeadSObject();
        await MapLead(bizFormItem, lead, fieldMappings, converters);

        lead.LeadSource ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";
        lead.Company ??= "undefined"; //required field - set to 'undefined' to prevent errors

        var result = await apiService.CreateLeadAsync(lead);

        await syncItemService.LogFormLeadCreateItem(bizFormItem, result.Id!, CRMType.Salesforce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Salesforce, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    private async Task UpdateLeadAsync(string leadId, BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings,
        IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> converters)
    {
        var lead = new LeadSObject();
        await MapLead(bizFormItem, lead, fieldMappings, converters);

        lead.LeadSource ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";

        await apiService.UpdateLeadAsync(leadId, lead);

        await syncItemService.LogFormLeadUpdateItem(bizFormItem, leadId, CRMType.Salesforce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Salesforce, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    private async Task MapLead(BizFormItem bizFormItem, LeadSObject lead,
        IEnumerable<BizFormFieldMapping> fieldMappings, IEnumerable<ICRMTypeConverter<BizFormItem, LeadSObject>> converters)
    {
        foreach (var converter in converters)
        {
            await converter.Convert(bizFormItem, lead);
        }

        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.FormFieldMapping.MapFormField(bizFormItem);
            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => lead.AdditionalProperties[m.CrmFieldName] = formFieldValue,
                CRMFieldMappingFunction<LeadSObject> m => m.MapCrmField(lead, formFieldValue),
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMappings),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }
}