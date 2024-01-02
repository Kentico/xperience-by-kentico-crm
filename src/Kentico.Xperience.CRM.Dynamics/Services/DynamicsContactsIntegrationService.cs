using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
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

    public async Task CreateLeadAsync(ContactInfo contactInfo)
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
            logger.LogError(e, "Create entity failed - api error: {ApiResult}", e.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Create entity failed - api error: {ApiResult}", ie.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Create entity failed - unknown api error");
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
    }

    public Task UpdateLeadAsync(ContactInfo contactInfo)
    {
        throw new NotImplementedException();
    }

    public Task CreateContactAsync(ContactInfo contactInfo)
    {
        throw new NotImplementedException();
    }

    public Task UpdateContactAsync(ContactInfo contactInfo)
    {
        throw new NotImplementedException();
    }

    private async Task CreateEntity(ContactInfo contactInfo, string entityType)
    {
        try
        {
            if (!await validationService.ValidateContactInfo(contactInfo))
            {
                logger.LogInformation("Contact info {ItemID} refused by validation",
                    contactInfo.ContactID);
                return;
            }
            var leadEntity = new Entity(entityType);
            MapCRMEntity(contactInfo, leadEntity, contactMapping.FieldsMapping);
            if (entityType == Lead.EntityLogicalName)
            {
                leadEntity["subject"] ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";    
            }

            await serviceClient.CreateAsync(leadEntity);
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Create entity failed - api error: {ApiResult}", e.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Create entity failed - api error: {ApiResult}", ie.Detail);
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Create entity failed - unknown api error");
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }
    }

    protected virtual void MapCRMEntity(ContactInfo contactInfo, Entity leadEntity,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.ContactFieldMapping.MapContactField(contactInfo);

            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => leadEntity[m.CrmFieldName] = formFieldValue,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }
}