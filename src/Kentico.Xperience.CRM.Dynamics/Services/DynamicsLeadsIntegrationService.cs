using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Services.Implementations;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Microsoft.Extensions.Logging;
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

    public DynamicsLeadsIntegrationService(
        DynamicsBizFormsMappingConfiguration bizFormMappingConfig, ILeadsIntegrationValidationService validationService,
        ServiceClient serviceClient,
        ILogger<DynamicsLeadsIntegrationService> logger)
        : base(bizFormMappingConfig, validationService, logger)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;
        this.serviceClient = serviceClient;
        this.logger = logger;
    }

    protected override async Task CreateLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        try
        {
            var leadEntity = new Lead();
            MapLead(bizFormItem, leadEntity, fieldMappings);

            if (leadEntity.Subject is null)
            {
                leadEntity.Subject = $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";
            }

            if (bizFormMappingConfig.ExternalIdFieldName is { Length: > 0 } externalIdFieldName)
            {
                leadEntity[externalIdFieldName] = FormatExternalId(bizFormItem);
            }

            await serviceClient.CreateAsync(leadEntity);
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", e.Detail);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", ie.Detail);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Create lead failed - unknown api error");
        }
    }

    protected override async Task UpdateLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        try
        {
            var leadEntity = await GetLeadByExternalId(FormatExternalId(bizFormItem));
            if (leadEntity is not null)
            {
                MapLead(bizFormItem, leadEntity, fieldMappings);
                await serviceClient.UpdateAsync(leadEntity);
            }
            else
            {
                await CreateLeadAsync(bizFormItem, fieldMappings);
            }
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Update lead failed - api error: {ApiResult}", e.Detail);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Update lead failed - api error: {ApiResult}", ie.Detail);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Update lead failed - unknown api error");
        }
    }

    protected virtual void MapLead(BizFormItem bizFormItem, Lead leadEntity,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.FormFieldMapping.MapFormField(bizFormItem);

            _ = fieldMapping.CrmFieldMapping switch
            {
                CrmFieldNameMapping m => leadEntity[m.CrmFieldName] = formFieldValue,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CrmFieldMapping), fieldMapping.CrmFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }

    private async Task<Lead?> GetLeadByExternalId(string externalId)
    {
        var query = new QueryExpression(Lead.EntityLogicalName) { ColumnSet = new ColumnSet(true), TopCount = 1 };
        query.Criteria.AddCondition(bizFormMappingConfig.ExternalIdFieldName, ConditionOperator.Equal, externalId);

        return (await serviceClient.RetrieveMultipleAsync(query)).Entities.FirstOrDefault()?.ToEntity<Lead>();
    }
}