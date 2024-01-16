using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;

namespace Kentico.Xperience.CRM.Dynamics.Workers;

internal class ContactsSyncQueueWorker : ContactsSyncQueueWorkerBase<ContactsSyncQueueWorker,
    IDynamicsContactsIntegrationService, DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}