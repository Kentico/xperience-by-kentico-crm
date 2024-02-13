using CMS.ContactManagement;
using CMS.Helpers;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salesforce.OpenApi;
using System.Text.Json;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

internal class SalesforceContactsIntegrationService : ISalesforceContactsIntegrationService
{
    private readonly SalesforceContactMappingConfiguration contactMapping;
    private readonly IContactsIntegrationValidationService validationService;
    private readonly ISalesforceApiService apiService;
    private readonly ILogger<SalesforceContactsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsSnapshot<SalesforceIntegrationSettings> settings;
    private readonly IEnumerable<ICRMTypeConverter<ContactInfo, LeadSObject>> contactLeadConverters;
    private readonly IEnumerable<ICRMTypeConverter<ContactInfo, ContactSObject>> contactContactConverters;
    private readonly IEnumerable<ICRMTypeConverter<LeadSObject, ContactInfo>> leadKenticoConverters;
    private readonly IEnumerable<ICRMTypeConverter<ContactSObject, ContactInfo>> contactKenticoConverters;
    private readonly IContactInfoProvider contactInfoProvider;

    public SalesforceContactsIntegrationService(
        SalesforceContactMappingConfiguration contactMapping,
        IContactsIntegrationValidationService validationService,
        ISalesforceApiService apiService,
        ILogger<SalesforceContactsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsSnapshot<SalesforceIntegrationSettings> settings,
        IEnumerable<ICRMTypeConverter<ContactInfo, LeadSObject>> contactLeadConverters,
        IEnumerable<ICRMTypeConverter<ContactInfo, ContactSObject>> contactContactConverters,
        IEnumerable<ICRMTypeConverter<LeadSObject, ContactInfo>> leadKenticoConverters,
        IEnumerable<ICRMTypeConverter<ContactSObject, ContactInfo>> contactKenticoConverters,
        IContactInfoProvider contactInfoProvider)
    {
        this.contactMapping = contactMapping;
        this.validationService = validationService;
        this.apiService = apiService;
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

            var syncItem = await syncItemService.GetContactSyncItem(contactInfo, CRMType.Salesforce);

            if (syncItem is null)
            {
                await UpdateLeadByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
            }
            else
            {
                LeadSObject? existingLead = null;
                try
                {
                    existingLead = await apiService.GetLeadById(syncItem.CRMSyncItemCRMID, nameof(LeadSObject.Id));
                }
                catch (Exception)
                {
                    //exception means de-facto 404-NotFound status
                }

                if (existingLead is null)
                {
                    await UpdateLeadByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
                }
                else if (!settings.Value.IgnoreExistingRecords)
                {
                    await UpdateLeadAsync(existingLead.Id!, contactInfo, contactMapping.FieldsMapping);
                }
                else
                {
                    logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
                }
            }
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Salesforce);
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Create lead failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Salesforce);
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Create lead failed - unexpected api error");
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Salesforce);
        }
    }

    public async Task SynchronizeContactToContactsAsync(ContactInfo contactInfo)
    {
        try
        {
            if (!await validationService.ValidateContactInfo(contactInfo))
            {
                logger.LogInformation("Contact {ContactEmail} refused by validation",
                    contactInfo.ContactEmail);
                return;
            }

            var syncItem = await syncItemService.GetContactSyncItem(contactInfo, CRMType.Salesforce);

            if (syncItem is null)
            {
                await UpdateContactByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
            }
            else
            {
                ContactSObject? existingContact = null;
                try
                {
                    existingContact =
                        await apiService.GetContactById(syncItem.CRMSyncItemCRMID, nameof(ContactSObject.Id));
                }
                catch (ApiException e) when (e.StatusCode == 404)
                {
                    //supress exception on 404-NotFound status
                }

                if (existingContact is null)
                {
                    await UpdateContactByEmailOrCreate(contactInfo, contactMapping.FieldsMapping);
                }
                else if (!settings.Value.IgnoreExistingRecords)
                {
                    await UpdateContactAsync(existingContact.Id!, contactInfo, contactMapping.FieldsMapping);
                }
                else
                {
                    logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
                }
            }
        }
        catch (ApiException<ICollection<RestApiError>> e)
        {
            logger.LogError(e, "Create contact failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Salesforce);
        }
        catch (ApiException<ICollection<ErrorInfo>> e)
        {
            logger.LogError(e, "Create contact failed - api error: {ApiResult}", JsonSerializer.Serialize(e.Result));
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Salesforce);
        }
        catch (ApiException e)
        {
            logger.LogError(e, "Create contact failed - unexpected api error");
            failedSyncItemService.LogFailedContactItem(contactInfo, CRMType.Salesforce);
        }
    }

    public async Task SynchronizeLeadsToKenticoAsync(DateTime lastSync)
    {
        RequestStockHelper.Add("SuppressEvents", true);
        var leads = await apiService.GetModifiedLeadsAsync(lastSync);
        foreach (var lead in leads)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lead.Email))
                {
                    continue;
                }

                var contactInfo = (await contactInfoProvider.Get()
                    .WhereEquals(nameof(ContactInfo.ContactEmail), lead.Email)
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
                    await syncItemService.LogContactSyncItem(contactInfo, lead.Id!, CRMType.Salesforce);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Syncing to contact info {ContactEmail} failed", lead.Email);
            }
        }
    }

    public async Task SynchronizeContactsToKenticoAsync(DateTime lastSync)
    {
        RequestStockHelper.Add("SuppressEvents", true);
        var contacts = await apiService.GetModifiedContactsAsync(lastSync);
        foreach (var contact in contacts)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contact.Email))
                {
                    continue;
                }

                var contactInfo = (await contactInfoProvider.Get()
                    .WhereEquals(nameof(ContactInfo.ContactEmail), contact.Email)
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
                    await syncItemService.LogContactSyncItem(contactInfo, contact.Id!, CRMType.Salesforce);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Syncing to contact info {ContactEmail} failed", contact.Email);
            }
        }
    }

    private async Task UpdateLeadByEmailOrCreate(ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        string? existingLeadId = null;

        var tmpLead = new LeadSObject();
        await MapLead(contactInfo, tmpLead, fieldMappings);

        string? emailAddress = tmpLead.Email;
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            emailAddress = tmpLead.AdditionalProperties.TryGetValue(nameof(LeadSObject.Email), out var email) ?
                email as string :
                null;
        }
        if (!string.IsNullOrWhiteSpace(emailAddress))
        {
            existingLeadId = await apiService.GetLeadByEmail(emailAddress!);
        }

        if (existingLeadId is null)
        {
            await CreateLeadAsync(contactInfo, fieldMappings);
        }
        else if (!settings.Value.IgnoreExistingRecords)
        {
            await UpdateLeadAsync(existingLeadId, contactInfo, fieldMappings);
        }
        else
        {
            logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
        }
    }

    private async Task UpdateContactByEmailOrCreate(ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        string? existingLeadId = null;

        var tmpLead = new ContactSObject();
        await MapContact(contactInfo, tmpLead, fieldMappings);

        string? emailAddress = tmpLead.Email;
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            emailAddress = tmpLead.AdditionalProperties.TryGetValue(nameof(ContactSObject.Email), out var email) ?
                email as string :
                null;
        }
        if (!string.IsNullOrWhiteSpace(emailAddress))
        {
            existingLeadId = await apiService.GetContactByEmail(emailAddress);
        }

        if (existingLeadId is null)
        {
            await CreateContactAsync(contactInfo, fieldMappings);
        }
        else if (!settings.Value.IgnoreExistingRecords)
        {
            await UpdateContactAsync(existingLeadId, contactInfo, fieldMappings);
        }
        else
        {
            logger.LogInformation("Contact {ContactEmail} ignored", contactInfo.ContactEmail);
        }
    }

    private async Task CreateLeadAsync(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var lead = new LeadSObject();
        await MapLead(contactInfo, lead, fieldMappings);

        lead.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        if (string.IsNullOrWhiteSpace(lead.Company))
        {
            lead.Company = "undefined"; //required field - set to 'undefined' to prevent errors
        }

        var result = await apiService.CreateLeadAsync(lead);

        await syncItemService.LogContactSyncItem(contactInfo, result.Id!, CRMType.Salesforce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Salesforce, contactInfo.TypeInfo.ObjectClassName,
            contactInfo.ContactID);
    }

    private async Task UpdateLeadAsync(string leadId, ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var lead = new LeadSObject();
        await MapLead(contactInfo, lead, fieldMappings);

        lead.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        await apiService.UpdateLeadAsync(leadId, lead);

        await syncItemService.LogContactSyncItem(contactInfo, leadId, CRMType.Salesforce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Salesforce, contactInfo.TypeInfo.ObjectClassName,
            contactInfo.ContactID);
    }

    private async Task CreateContactAsync(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var contact = new ContactSObject();
        await MapContact(contactInfo, contact, fieldMappings);

        contact.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        var result = await apiService.CreateContactAsync(contact);

        await syncItemService.LogContactSyncItem(contactInfo, result.Id!, CRMType.Salesforce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Salesforce, contactInfo.TypeInfo.ObjectClassName,
            contactInfo.ContactID);
    }

    private async Task UpdateContactAsync(string leadId, ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var contact = new ContactSObject();
        await MapContact(contactInfo, contact, fieldMappings);

        contact.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        await apiService.UpdateContactAsync(leadId, contact);

        await syncItemService.LogContactSyncItem(contactInfo, leadId, CRMType.Salesforce);
        failedSyncItemService.DeleteFailedSyncItem(CRMType.Salesforce, contactInfo.TypeInfo.ObjectClassName,
            contactInfo.ContactID);
    }

    protected async Task MapLead(ContactInfo contactInfo, LeadSObject lead,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var converter in contactLeadConverters)
        {
            await converter.Convert(contactInfo, lead);
        }

        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.ContactFieldMapping.MapContactField(contactInfo);
            if (formFieldValue is string val && string.IsNullOrWhiteSpace(val))
            {
                //dot not try to set empty value and send them as null to prevent api errors
                continue;
            }

            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => lead.AdditionalProperties[m.CrmFieldName] = formFieldValue,
                CRMFieldMappingFunction<LeadSObject> m => m.MapCrmField(lead, formFieldValue),
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }

    protected async Task MapContact(ContactInfo contactInfo, ContactSObject contact,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var converter in contactContactConverters)
        {
            await converter.Convert(contactInfo, contact);
        }

        foreach (var fieldMapping in fieldMappings)
        {
            var formFieldValue = fieldMapping.ContactFieldMapping.MapContactField(contactInfo);
            _ = fieldMapping.CRMFieldMapping switch
            {
                CRMFieldNameMapping m => contact.AdditionalProperties[m.CrmFieldName] = formFieldValue,
                CRMFieldMappingFunction<ContactSObject> m => m.MapCrmField(contact, formFieldValue),
                _ => throw new ArgumentOutOfRangeException(nameof(fieldMapping.CRMFieldMapping),
                    fieldMapping.CRMFieldMapping.GetType(), "Unsupported mapping")
            };
        }
    }
}