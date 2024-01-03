using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

namespace Kentico.Xperience.CRM.Dynamics.Services;

public interface IDynamicsContactsIntegrationService : IContactsIntegrationService
{
    Task<IEnumerable<Lead>> GetModifiedLeadsAsync(DateTime lastSync);
    Task<IEnumerable<Contact>> GetModifiedContactsAsync(DateTime lastSync);
}