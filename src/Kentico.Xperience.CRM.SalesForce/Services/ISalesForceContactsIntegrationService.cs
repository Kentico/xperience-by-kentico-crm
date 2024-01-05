using Kentico.Xperience.CRM.Common.Services;
using SalesForce.OpenApi;

namespace Kentico.Xperience.CRM.SalesForce.Services;

public interface ISalesForceContactsIntegrationService : IContactsIntegrationService
{
    Task<IEnumerable<LeadSObject>> GetModifiedLeadsAsync(DateTime lastSync);
    Task<IEnumerable<ContactSObject>> GetModifiedContactsAsync(DateTime lastSync);
}