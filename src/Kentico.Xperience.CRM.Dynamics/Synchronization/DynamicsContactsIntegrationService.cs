using CMS.ContactManagement;
using CMS.Helpers;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;

namespace Kentico.Xperience.CRM.Dynamics.Synchronization;

internal class DynamicsContactsIntegrationService : IDynamicsContactsIntegrationService
{
    private readonly DynamicsContactMappingConfiguration contactMapping;
    private readonly IContactsIntegrationValidationService validationService;
    private readonly ServiceClient serviceClient;
    private readonly ILogger<DynamicsContactsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsSnapshot<DynamicsIntegrationSettings> settings;
    private readonly IEnumerable<ICRMTypeConverter<ContactInfo, Lead>> contactLeadConverters;
    private readonly IEnumerable<ICRMTypeConverter<ContactInfo, Contact>> contactContactConverters;
    private readonly IEnumerable<ICRMTypeConverter<Lead, ContactInfo>> leadKenticoConverters;
    private readonly IEnumerable<ICRMTypeConverter<Contact, ContactInfo>> contactKenticoConverters;
    private readonly IContactInfoProvider contactInfoProvider;

    public DynamicsContactsIntegrationService(DynamicsContactMappingConfiguration contactMapping,
        IContactsIntegrationValidationService validationService,
        ServiceClient serviceClient,
        ILogger<DynamicsContactsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsSnapshot<DynamicsIntegrationSettings> settings,
        IEnumerable<ICRMTypeConverter<ContactInfo, Lead>> contactLeadConverters,
        IEnumerable<ICRMTypeConverter<ContactInfo, Contact>> contactContactConverters,
        IEnumerable<ICRMTypeConverter<Lead, ContactInfo>> leadKenticoConverters,
        IEnumerable<ICRMTypeConverter<Contact, ContactInfo>> contactKenticoConverters,
        IContactInfoProvider contactInfoProvider)
    {
        this.contactMapping = contactMapping;
        this.validationService = validationService;
        this.serviceClient = serviceClient;
        this.logger = logger;
        this.syncItemService = syncItemService;
        this.failedSyncItemService = failedSyncItemService;
        this.settings = settings;
        this.contactLeadConverters = contactLeadConverters;
        this.contactContactConverters = contactContactConverters;
        this.leadKenticoConverters = leadKenticoConverters;
        this.contactKenticoConverters = contactKenticoConverters;
        this.contactInfoProvider = contactInfoProvider;
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

            var syncItem = await syncItemService.GetContactSyncItem(contactInfo, CRMType.Dynamics);

            if (syncItem is null)
            {
                await UpdateLeadByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
            }
            else
            {
                var existingLead =
                    await GetEntityById<Lead>(Guid.Parse(syncItem.CRMSyncItemCRMID), Lead.EntityLogicalName);
                if (existingLead is null)
                {
                    await UpdateLeadByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
                }
                else if (!settings.Value.IgnoreExistingRecords)
                {
                    await UpdateLeadAsync(existingLead, contactInfo, contactMapping.FieldsMapping);
                }
                else
                {
                    logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
                }
            }
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

            var syncItem = await syncItemService.GetContactSyncItem(contactInfo, CRMType.Dynamics);

            if (syncItem is null)
            {
                await UpdateContactByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
            }
            else
            {
                var existingContact =
                    await GetEntityById<Contact>(Guid.Parse(syncItem.CRMSyncItemCRMID), Contact.EntityLogicalName);
                if (existingContact is null)
                {
                    await UpdateContactByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
                }
                else if (!settings.Value.IgnoreExistingRecords)
                {
                    await UpdateContactAsync(existingContact, contactInfo, contactMapping.FieldsMapping);
                }
                else
                {
                    logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
                }
            }
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


    public async Task SynchronizeLeadsToKenticoAsync(DateTime lastSync)
    {
        RequestStockHelper.Add("SuppressEvents", true);
        var leads = await GetModifiedLeadsAsync(lastSync);
        foreach (var lead in leads)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lead.EMailAddress1))
                {
                    continue;
                }

                var contactInfo = (await contactInfoProvider.Get()
                    .WhereEquals(nameof(ContactInfo.ContactEmail), lead.EMailAddress1)
                    .TopN(1)
                    .GetEnumerableTypedResultAsync())?.FirstOrDefault();

                if (contactInfo is null)
                {
                    contactInfo = new ContactInfo();
                }

                foreach (var converter in leadKenticoConverters)
                {
                    await converter.Convert(lead, contactInfo);
                }

                if (contactInfo.HasChanged)
                {
                    contactInfoProvider.Set(contactInfo);
                    await syncItemService.LogContactSyncItem(contactInfo, lead.Id.ToString(), CRMType.Dynamics);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Syncing to contact info {ContactEmail} failed", lead.EMailAddress1);
            }
        }
    }

    public async Task SynchronizeContactsToKenticoAsync(DateTime lastSync)
    {
        RequestStockHelper.Add("SuppressEvents", true);
        var contacts = await GetModifiedContactsAsync(lastSync);
        foreach (var contact in contacts)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contact.EMailAddress1))
                {
                    continue;
                }

                var contactInfo = (await contactInfoProvider.Get()
                    .WhereEquals(nameof(ContactInfo.ContactEmail), contact.EMailAddress1)
                    .TopN(1)
                    .GetEnumerableTypedResultAsync())?.FirstOrDefault();

                if (contactInfo is null)
                {
                    contactInfo = new ContactInfo();
                }

                foreach (var converter in contactKenticoConverters)
                {
                    await converter.Convert(contact, contactInfo);
                }

                if (contactInfo.HasChanged)
                {
                    contactInfoProvider.Set(contactInfo);
                    await syncItemService.LogContactSyncItem(contactInfo, contact.Id.ToString(), CRMType.Dynamics);
                }

            }
            catch (Exception e)
            {
                logger.LogError(e, "Syncing to contact info {ContactEmail} failed", contact.EMailAddress1);
            }
        }
    }

    private async Task UpdateLeadByEmailOrCreate(ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        Lead? existingLead = null;
        var tmpLead = new Lead();
        await MapCRMEntity(contactInfo, tmpLead, fieldMappings);

        if (!string.IsNullOrWhiteSpace(tmpLead.EMailAddress1))
        {
            existingLead = await GetEntityByEmail<Lead>(tmpLead.EMailAddress1, Lead.EntityLogicalName);
        }

        if (existingLead is null)
        {
            await CreateLeadAsync(contactInfo, fieldMappings);
        }
        else if (!settings.Value.IgnoreExistingRecords)
        {
            await UpdateLeadAsync(existingLead, contactInfo, fieldMappings);
        }
        else
        {
            logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
        }
    }

    private async Task UpdateContactByEmailOrCreate(ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        Contact? existingContact = null;
        var tmpContact = new Contact();
        await MapCRMEntity(contactInfo, tmpContact, fieldMappings);

        if (!string.IsNullOrWhiteSpace(tmpContact.EMailAddress1))
        {
            existingContact = await GetEntityByEmail<Contact>(tmpContact.EMailAddress1, Contact.EntityLogicalName);
        }

        if (existingContact is null)
        {
            await CreateContactAsync(contactInfo, fieldMappings);
        }
        else if (!settings.Value.IgnoreExistingRecords)
        {
            await UpdateContactAsync(existingContact, contactInfo, fieldMappings);
        }
        else
        {
            logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
        }
    }

    private async Task CreateLeadAsync(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var leadEntity = new Lead();
        await MapCRMEntity(contactInfo, leadEntity, fieldMappings);

        leadEntity.Subject ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        var leadId = await serviceClient.CreateAsync(leadEntity);

        await syncItemService.LogContactSyncItem(contactInfo, leadId.ToString(), CRMType.Dynamics,
            createdByKentico: true);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, contactInfo.ClassName, contactInfo.ContactID);
    }

    private async Task UpdateLeadAsync(Lead leadEntity, ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        await MapCRMEntity(contactInfo, leadEntity, fieldMappings);

        leadEntity.Subject ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        await serviceClient.UpdateAsync(leadEntity);

        await syncItemService.LogContactSyncItem(contactInfo, leadEntity.LeadId.ToString()!, CRMType.Dynamics);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, contactInfo.ClassName, contactInfo.ContactID);
    }

    private async Task CreateContactAsync(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var contactEntity = new Contact();
        await MapCRMEntity(contactInfo, contactEntity, fieldMappings);

        var leadId = await serviceClient.CreateAsync(contactEntity);

        await syncItemService.LogContactSyncItem(contactInfo, leadId.ToString(), CRMType.Dynamics,
            createdByKentico: true);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, contactInfo.ClassName, contactInfo.ContactID);
    }

    private async Task UpdateContactAsync(Contact contactEntity, ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        await MapCRMEntity(contactInfo, contactEntity, fieldMappings);

        await serviceClient.UpdateAsync(contactEntity);

        await syncItemService.LogContactSyncItem(contactInfo, contactEntity.ContactId.ToString()!, CRMType.Dynamics);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Dynamics, contactInfo.ClassName, contactInfo.ContactID);
    }

    private async Task MapCRMEntity(ContactInfo contactInfo, Entity leadEntity,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        if (leadEntity is Lead lead)
        {
            foreach (var converter in contactLeadConverters)
            {
                await converter.Convert(contactInfo, lead);
            }
        }

        if (leadEntity is Contact contact)
        {
            foreach (var converter in contactContactConverters)
            {
                await converter.Convert(contactInfo, contact);
            }
        }

        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.ContactFieldMapping.MapContactField(contactInfo);

            if (fieldMapping.CRMFieldMapping is CRMFieldNameMapping m)
            {
                leadEntity[m.CrmFieldName] = formFieldValue;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fieldMappings),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping");
            }
        }
    }

    private async Task<IEnumerable<Lead>> GetModifiedLeadsAsync(DateTime lastSync)
    {
        return await GetModifiedEntitiesAsync<Lead>(lastSync, Lead.EntityLogicalName);
    }

    private async Task<IEnumerable<Contact>> GetModifiedContactsAsync(DateTime lastSync)
    {
        return await GetModifiedEntitiesAsync<Contact>(lastSync, Contact.EntityLogicalName);
    }

    private async Task<IEnumerable<TEntity>> GetModifiedEntitiesAsync<TEntity>(DateTime lastSync, string entityName)
        where TEntity : Entity
    {
        try
        {
            var query = new QueryExpression(entityName) { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("modifiedon", ConditionOperator.GreaterThan, lastSync.ToUniversalTime());

            return (await serviceClient.RetrieveMultipleAsync(query)).Entities.Select(e => e.ToEntity<TEntity>())
                .ToList();
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            logger.LogError(e, "Get modified entities - api error: {ApiResult}", e.Detail);
            return Enumerable.Empty<TEntity>();
        }
        catch (Exception e) when (e.InnerException is FaultException<OrganizationServiceFault> ie)
        {
            logger.LogError(e, "Get modified entities - api error: {ApiResult}", ie.Detail);
            return Enumerable.Empty<TEntity>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Get modified entities - unknown api error");
            return Enumerable.Empty<TEntity>();
        }
    }

    private async Task<TEntity?> GetEntityById<TEntity>(Guid leadId, string logicalName)
        where TEntity : Entity
        => (await serviceClient.RetrieveAsync(logicalName, leadId, new ColumnSet(true)))?.ToEntity<TEntity>();

    private async Task<TEntity?> GetEntityByEmail<TEntity>(string email, string logicalName)
        where TEntity : Entity
    {
        var query = new QueryExpression(logicalName) { ColumnSet = new ColumnSet(true), TopCount = 1 };
        query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);

        return (await serviceClient.RetrieveMultipleAsync(query)).Entities.FirstOrDefault()?.ToEntity<TEntity>();
    }
}