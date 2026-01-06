using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Dynamics.Configuration;

namespace Kentico.Xperience.CRM.Dynamics.Synchronization;

/// <summary>
/// Specific thread worker for Dynamics which try to synchronize failed items. It run each 1 minute.
/// </summary>
internal class FailedItemsWorker : FailedSyncItemsWorkerBase<FailedItemsWorker, IDynamicsLeadsIntegrationService,
    IDynamicsContactsIntegrationService,
    DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}