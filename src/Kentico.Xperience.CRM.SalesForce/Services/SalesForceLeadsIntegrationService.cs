using CMS.OnlineForms;
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

    public SalesForceLeadsIntegrationService(
        SalesForceBizFormsMappingConfiguration bizFormMappingConfig,
        ILeadsIntegrationValidationService validationService,
        ISalesForceApiService apiService,
        ILogger<SalesForceLeadsIntegrationService> logger)
        : base(bizFormMappingConfig, validationService, logger)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;        
        this.apiService = apiService;
        this.logger = logger;
    }

    protected override async Task CreateLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        try
        {
            var lead = new LeadSObject();
            MapLead(bizFormItem, lead, fieldMappings);

            lead.LeadSource ??= $"Form {bizFormItem.BizFormInfo.FormDisplayName} - ID: {bizFormItem.ItemID}";
            lead.Company ??= "undefined"; //required field - set to 'undefined' to prevent errors
            if (bizFormMappingConfig.ExternalIdFieldName is { Length: > 0 } externalIdFieldName)
            {
                lead.AdditionalProperties[externalIdFieldName] = FormatExternalId(bizFormItem);
            }

            await apiService.CreateLeadAsync(lead);
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Create lead failed - unexpected api error");
        }
    }

    protected override async Task UpdateLeadAsync(BizFormItem bizFormItem,
        IEnumerable<BizFormFieldMapping> fieldMappings)
    {
        try
        {
            string? leadId = await apiService.GetLeadIdByExternalId(bizFormMappingConfig.ExternalIdFieldName!,
                FormatExternalId(bizFormItem));

            if (leadId is not null)
            {
                var lead = new LeadSObject();
                MapLead(bizFormItem, lead, fieldMappings);
                await apiService.UpdateLeadAsync(leadId, lead);
            }
            else
            {
                await CreateLeadAsync(bizFormItem, fieldMappings);
            }
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Update lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Update lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Update lead failed - unexpected api error");
        }
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
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping), fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }
}