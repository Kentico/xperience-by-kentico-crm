using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

/// <summary>
/// Http typed client for Salesforce REST API
/// </summary>
public interface ISalesforceApiService
{
    /// <summary>
    /// Creates lead entity to Salesforce Leads
    /// </summary>
    /// <param name="lead"></param>
    /// <returns></returns>
    Task<SaveResult> CreateLeadAsync(LeadSObject lead);

    /// <summary>
    /// Updates lead entity to Salesforce Leads 
    /// </summary>
    /// <param name="id">Salesforce lead ID</param>
    /// <param name="leadSObject"></param>
    /// <returns></returns>
    Task UpdateLeadAsync(string id, LeadSObject leadSObject);
    
    /// <summary>
    /// Get Lead by primary Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    Task<LeadSObject?> GetLeadById(string id, string? fields = null);

    /// <summary>
    /// Get Lead by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<string?> GetLeadByEmail(string email);
    
    /// <summary>
    /// Creates lead entity to SalesForce Leads
    /// </summary>
    /// <param name="lead"></param>
    /// <returns></returns>
    Task<SaveResult> CreateContactAsync(ContactSObject contact);

    /// <summary>
    /// Updates lead entity to SalesForce Leads 
    /// </summary>
    /// <param name="id">SalesForce lead ID</param>
    /// <param name="leadSObject"></param>
    /// <returns></returns>
    Task UpdateContactAsync(string id, ContactSObject contact);
    
    /// <summary>
    /// Get Lead by primary Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    Task<ContactSObject?> GetContactById(string id, string? fields = null);

    /// <summary>
    /// Get Lead by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<string?> GetContactByEmail(string email);
}