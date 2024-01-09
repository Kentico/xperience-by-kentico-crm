using SalesForce.OpenApi;

namespace Kentico.Xperience.CRM.SalesForce.Services;

/// <summary>
/// Http typed client for SalesForce REST API
/// </summary>
public interface ISalesForceApiService
{
    /// <summary>
    /// Creates lead entity to SalesForce Leads
    /// </summary>
    /// <param name="lead"></param>
    /// <returns></returns>
    Task<SaveResult> CreateLeadAsync(LeadSObject lead);

    /// <summary>
    /// Updates lead entity to SalesForce Leads 
    /// </summary>
    /// <param name="id">SalesForce lead ID</param>
    /// <param name="leadSObject"></param>
    /// <returns></returns>
    Task UpdateLeadAsync(string id, LeadSObject leadSObject);

    /// <summary>
    /// Get Lead ID for item by external ID
    /// </summary>
    /// <param name="fieldName">Custom field for external ID</param>
    /// <param name="externalId">External ID value</param>
    /// <returns></returns>
    Task<string?> GetLeadIdByExternalId(string fieldName, string externalId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<LeadSObject?> GetLeadById(string id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<string?> GetLeadByEmail(string email);
}