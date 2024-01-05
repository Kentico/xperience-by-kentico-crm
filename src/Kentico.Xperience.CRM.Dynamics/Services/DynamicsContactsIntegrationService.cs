using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;

namespace Kentico.Xperience.CRM.Dynamics.Services;

public class DynamicsContactsIntegrationService : IDynamicsContactsIntegrationService
{
    private readonly DynamicsContactMappingConfiguration contactMapping;
    private readonly IContactsIntegrationValidationService validationService;
    private readonly ServiceClient serviceClient;
    private readonly ILogger<DynamicsContactsIntegrationService> logger;
    private readonly IFailedSyncItemService failedSyncItemService;

    public DynamicsContactsIntegrationService(DynamicsContactMappingConfiguration contactMapping,
        IContactsIntegrationValidationService validationService,
        ServiceClient serviceClient,
        ILogger<DynamicsContactsIntegrationService> logger,
        IFailedSyncItemService failedSyncItemService)
    {
        this.contactMapping = contactMapping;
        this.validationService = validationService;
        this.serviceClient = serviceClient;
        this.logger = logger;
        this.failedSyncItemService = failedSyncItemService;
    }

    public async Task SynchronizeContactToLeadsAsync(ContactInfo contactInfo)
    {
        try
        {
            if (!await validationService.ValidateContactInfo(contactInfo))
            {
                logger.LogInformation("Contact info {ItemID} refused by validation",
                    contactInfo.ContactID);
                return;
            }
            var leadEntity = new Lead();
            MapCRMEntity(contactInfo, leadEntity, contactMapping.FieldsMapping);

            leadEntity.Subject ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

            await serviceClient.CreateAsync(leadEntity);
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Lead sync failed - api error: {ApiResult}", e.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Lead sync failed - api error: {ApiResult}", ie.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Lead sync failed - unknown api error");
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
    }
    
    public async Task SynchronizeContactToContactsAsync(ContactInfo contactInfo)
    {
        try
        {
            if (!await validationService.ValidateContactInfo(contactInfo))
            {
                logger.LogInformation("Contact info {ItemID} refused by validation",
                    contactInfo.ContactID);
                return;
            }
            var leadEntity = new Contact();
            MapCRMEntity(contactInfo, leadEntity, contactMapping.FieldsMapping);

            //leadEntity.Subject ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

            await serviceClient.CreateAsync(leadEntity);
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Contact sync failed - api error: {ApiResult}", e.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Contact sync  failed - api error: {ApiResult}", ie.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Contact sync  failed - unknown api error");
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
    }
    
    protected virtual void MapCRMEntity(ContactInfo contactInfo, Entity leadEntity,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.ContactFieldMapping.MapContactField(contactInfo);

            if (fieldMapping.CRMFieldMapping is CRMFieldNameMapping m)
            {
                leadEntity[m.CrmFieldName] = formFieldValue;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping");
            }
        }
    }

    public async Task<IEnumerable<Lead>> GetModifiedLeadsAsync(DateTime lastSync)
    {
        return await GetModifiedEntitiesAsync<Lead>(lastSync, Lead.EntityLogicalName);
    }

    public async Task<IEnumerable<Contact>> GetModifiedContactsAsync(DateTime lastSync)
    {
        return await GetModifiedEntitiesAsync<Contact>(lastSync, Contact.EntityLogicalName);
    }

    private async Task<IEnumerable<TEntity>> GetModifiedEntitiesAsync<TEntity>(DateTime lastSync, string entityName)
        where TEntity : Entity
    {
        var query = new QueryExpression(entityName) { ColumnSet = new ColumnSet(true) };
        query.Criteria.AddCondition("modifiedon", ConditionOperator.GreaterThan, lastSync.ToUniversalTime());
        
        return (await serviceClient.RetrieveMultipleAsync(query)).Entities.Select(e => e.ToEntity<TEntity>());
    }
}