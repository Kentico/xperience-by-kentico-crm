using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Dynamics.Configuration;

namespace Kentico.Xperience.CRM.Dynamics.Synchronization;

internal class ContactsSyncQueueWorker : ContactsSyncQueueWorkerBase<ContactsSyncQueueWorker,
    IDynamicsContactsIntegrationService, DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}