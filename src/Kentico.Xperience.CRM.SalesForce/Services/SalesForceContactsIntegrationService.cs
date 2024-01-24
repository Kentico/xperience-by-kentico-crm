using CMS.ContactManagement;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesForce.OpenApi;
using System.Text.Json;

namespace Kentico.Xperience.CRM.SalesForce.Services;

internal class SalesForceContactsIntegrationService : ISalesForceContactsIntegrationService
{
    private readonly SalesForceContactMappingConfiguration contactMapping;
    private readonly IContactsIntegrationValidationService validationService;
    private readonly ISalesForceApiService apiService;
    private readonly ILogger<SalesForceContactsIntegrationService> logger;
    private readonly ICRMSyncItemService syncItemService;
    private readonly IFailedSyncItemService failedSyncItemService;
    private readonly IOptionsSnapshot<SalesForceIntegrationSettings> settings;
    private readonly IEnumerable<ICRMTypeConverter<ContactInfo, LeadSObject>> contactLeadConverters;
    private readonly IEnumerable<ICRMTypeConverter<ContactInfo, ContactSObject>> contactContactConverters;

    public SalesForceContactsIntegrationService(
        SalesForceContactMappingConfiguration contactMapping,
        IContactsIntegrationValidationService validationService,
        ISalesForceApiService apiService,
        ILogger<SalesForceContactsIntegrationService> logger,
        ICRMSyncItemService syncItemService,
        IFailedSyncItemService failedSyncItemService,
        IOptionsSnapshot<SalesForceIntegrationSettings> settings,
        IEnumerable<ICRMTypeConverter<ContactInfo, LeadSObject>> contactLeadConverters,
        IEnumerable<ICRMTypeConverter<ContactInfo, ContactSObject>> contactContactConverters)
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
    }

    public async Task SynchronizeContactToLeadsAsync(ContactInfo contactInfo)
    {
        try
        {
            var lead = new LeadSObject();
            MapLead(contactInfo, lead, contactMapping.FieldsMapping);

            lead.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";
            lead.Company ??= "undefined"; //required field - set to 'undefined' to prevent errors

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
    
    private async Task UpdateLeadByEmailOrCreate(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        string? existingLeadId = null;

        var tmpLead = new LeadSObject();
        await MapLead(contactInfo, tmpLead, fieldMappings);

        if (!string.IsNullOrWhiteSpace(tmpLead.Email))
        {
            existingLeadId = await apiService.GetLeadByEmail(tmpLead.Email);
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
    
    private async Task UpdateContactByEmailOrCreate(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        string? existingLeadId = null;

        var tmpLead = new ContactSObject();
        await MapContact(contactInfo, tmpLead, fieldMappings);

        if (!string.IsNullOrWhiteSpace(tmpLead.Email))
        {
            existingLeadId = await apiService.GetContactByEmail(tmpLead.Email);
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

        lead.Company ??= "undefined"; //required field - set to 'undefined' to prevent errors

        var result = await apiService.CreateLeadAsync(lead);

        await syncItemService.LogContactCreateItem(contactInfo, result.Id!, CRMType.SalesForce);
        //@TODO
        // failedSyncItemService.DeleteFailedSyncItem(CRMType.SalesForce, contactInfo.BizFormClassName,
        //     contactInfo.ItemID);
    }

    private async Task UpdateLeadAsync(string leadId, ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var lead = new LeadSObject();
        await MapLead(contactInfo, lead, fieldMappings);

        lead.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        await apiService.UpdateLeadAsync(leadId, lead);

        await syncItemService.LogContactUpdateItem(contactInfo, leadId, CRMType.SalesForce);
        //@TODO
        // failedSyncItemService.DeleteFailedSyncItem(CRMType.SalesForce, contactInfo.BizFormClassName,
        //     contactInfo.ItemID);
    }
    
    private async Task CreateContactAsync(ContactInfo contactInfo, IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var contact = new ContactSObject();
        await MapContact(contactInfo, contact, fieldMappings);

        contact.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";
        
        var result = await apiService.CreateContactAsync(contact);

        await syncItemService.LogContactCreateItem(contactInfo, result.Id!, CRMType.SalesForce);
        //@TODO
        // failedSyncItemService.DeleteFailedSyncItem(CRMType.SalesForce, contactInfo.BizFormClassName,
        //     contactInfo.ItemID);
    }

    private async Task UpdateContactAsync(string leadId, ContactInfo contactInfo,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        var contact = new ContactSObject();
        await MapContact(contactInfo, contact, fieldMappings);

        contact.LeadSource ??= $"Contact {contactInfo.ContactEmail} - ID: {contactInfo.ContactID}";

        await apiService.UpdateContactAsync(leadId, contact);

        await syncItemService.LogContactUpdateItem(contactInfo, leadId, CRMType.SalesForce);
        //@TODO
        // failedSyncItemService.DeleteFailedSyncItem(CRMType.SalesForce, contactInfo.BizFormClassName,
        //     contactInfo.ItemID);
    }

    protected async Task MapLead(ContactInfo contactInfo, LeadSObject lead,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var converter in contactLeadConverters)
        {
            lead = await converter.Convert(contactInfo, lead);
        }
        
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
    
    protected async Task MapContact(ContactInfo contactInfo, ContactSObject contact,
        IEnumerable<ContactFieldToCRMMapping> fieldMappings)
    {
        foreach (var converter in contactContactConverters)
        {
            contact = await converter.Convert(contactInfo, contact);
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