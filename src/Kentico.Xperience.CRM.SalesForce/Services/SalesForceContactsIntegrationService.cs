using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Microsoft.Extensions.Logging;
using SalesForce.OpenApi;
using System.Text.Json;

namespace Kentico.Xperience.CRM.SalesForce.Services;

internal class SalesForceContactsIntegrationService : ISalesForceContactsIntegrationService
{
    private readonly SalesForceContactMappingConfiguration contactMapping;
    private readonly IContactsIntegrationValidationService validationService;
    private readonly ISalesForceApiService apiService;
    private readonly ILogger<SalesForceContactsIntegrationService> logger;
    private readonly IFailedSyncItemService failedSyncItemService;

    public SalesForceContactsIntegrationService(
        SalesForceContactMappingConfiguration contactMapping,
        IContactsIntegrationValidationService validationService,
        ISalesForceApiService apiService,
        ILogger<SalesForceContactsIntegrationService> logger,
        IFailedSyncItemService failedSyncItemService)
    {
        this.contactMapping = contactMapping;
        this.validationService = validationService;
        this.apiService = apiService;
        this.logger = logger;
        this.failedSyncItemService = failedSyncItemService;
    }

    public async Task SynchronizeContactToLeadsAsync(ContactInfo contactInfo)
    {
        try
        {
            var lead = new LeadSObject();
            MapLead(contactInfo, lead, contactMapping.FieldsMapping);

            lead.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";
            lead.Company ??= "undefined"; //required field - set to 'undefined' to prevent errors
            // if (bizFormMappingConfig.ExternalIdFieldName is { Length: > 0 } externalIdFieldName)
            // {
            //     lead.AdditionalProperties[externalIdFieldName] = FormatExternalId(bizFormItem);
            // }

            await apiService.CreateLeadAsync(lead);
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            //failedSyncItemService.LogFailedLeadItem(contactInfo, CRMType.SalesForce);
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            //failedSyncItemService.LogFailedLeadItem(contactInfo, CRMType.SalesForce);
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Create lead failed - unexpected api error");
            //failedSyncItemService.LogFailedLeadItem(contactInfo, CRMType.SalesForce);
        }
    }

    public Task SynchronizeContactToContactsAsync(ContactInfo contactInfo)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<LeadSObject>> GetModifiedLeadsAsync(DateTime lastSync)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ContactSObject>> GetModifiedContactsAsync(DateTime lastSync)
    {
        throw new NotImplementedException();
    }

    protected virtual void MapLead(ContactInfo contactInfo, LeadSObject lead,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.ContactFieldMapping.MapContactField(contactInfo);
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