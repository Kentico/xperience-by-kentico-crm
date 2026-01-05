using System.ServiceModel;

using CMS.OnlineForms;

using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kentico.Xperience.CRM.Dynamics.Synchronization;

/// <summary>
/// Specific Lead integration service implementation for Dynamics Sales
/// </summary>
internal class DynamicsLeadsIntegrationService : IDynamicsLeadsIntegrationService
{
    private readonly DynamicsBizFormsMappingConfiguration bizFormMappingConfig;
    private readonly ILeadsIntegrationValidationService validationService;
    private readonly ServiceClient serviceClient;
    private readonly ILogger<DynamicsLeadsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsSnapshot<DynamicsIntegrationSettings> settings;
    private readonly IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> formsConverters;

    public DynamicsLeadsIntegrationService(
        DynamicsBizFormsMappingConfiguration bizFormMappingConfig,
        ILeadsIntegrationValidationService validationService,
        ServiceClient serviceClient,
        ILogger<DynamicsLeadsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsSnapshot<DynamicsIntegrationSettings> settings,
        IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> formsConverters)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;
        this.validationService = validationService;
        this.serviceClient = serviceClient;
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
        var leadConverters = Enumerable.Empty<ICRMTypeConverter<BizFormItem, Lead>>();
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
        IEnumerable<BizFormFieldMapping> fieldMappings, IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> converters)
    {
        try
        {
            var syncItem = await syncItemService.GetFormLeadSyncItem(bizFormItem, CRMType.Dynamics);

            if (syncItem is null)
            {
                await UpdateByEmailOrCreate(bizFormItem, fieldMappings, converters);
            }
            else
            {
                var existingLead = await GetLeadById(Guid.Parse(syncItem.CRMSyncItemCRMID));
                if (existingLead is null)
                {
                    await UpdateByEmailOrCreate(bizFormItem, fieldMappings, converters);
                }
                else if (!settings.Value.IgnoreExistingRecords)
                {
                    await UpdateLeadAsync(existingLead, bizFormItem, fieldMappings, converters);
                }
                else
                {
                    logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} ignored",
                        bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
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
        IEnumerable<BizFormFieldMapping> fieldMappings, IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> converters)
    {
        Lead? existingLead = null;
        var tmpLead = new Lead();
        await MapLead(bizFormItem, tmpLead, fieldMappings, converters);

        if (!string.IsNullOrWhiteSpace(tmpLead.EMailAddress1))
        {
            existingLead = await GetLeadByEmail(tmpLead.EMailAddress1);
        }

        if (existingLead is null)
        {
            await CreateLeadAsync(bizFormItem, fieldMappings, converters);
        }
        else if (!settings.Value.IgnoreExistingRecords)
        {
            await UpdateLeadAsync(existingLead, bizFormItem, fieldMappings, converters);
        }
        else
        {
            logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} ignored",
                bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
        }
    }

    private async Task CreateLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings, IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> converters)
    {
        var leadEntity = new Lead();
        await MapLead(bizFormItem, leadEntity, fieldMappings, converters);

        leadEntity.Subject ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";

        var leadId = await serviceClient.CreateAsync(leadEntity);

        await syncItemService.LogFormLeadCreateItem(bizFormItem, leadId.ToString(), CRMType.Dynamics);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    private async Task UpdateLeadAsync(Lead leadEntity, BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings, IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> converters)
    {
        await MapLead(bizFormItem, leadEntity, fieldMappings, converters);

        leadEntity.Subject ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";

        await serviceClient.UpdateAsync(leadEntity);

        await syncItemService.LogFormLeadUpdateItem(bizFormItem, leadEntity.LeadId.ToString()!, CRMType.Dynamics);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, bizFormItem.BizFormClassName,
            bizFormItem.ItemID);
    }

    protected static async Task MapLead(BizFormItem bizFormItem, Lead leadEntity,
        IEnumerable<BizFormFieldMapping> fieldMappings, IEnumerable<ICRMTypeConverter<BizFormItem, Lead>> converters)
    {
        foreach (var converter in converters)
        {
            await converter.Convert(bizFormItem, leadEntity);
        }

        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.FormFieldMapping.MapFormField(bizFormItem);

            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => leadEntity[m.CrmFieldName] = formFieldValue,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMappings),
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