using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;

namespace Kentico.Xperience.CRM.Dynamics.Workers;

/// <summary>
/// Specific thread worker for Dynamics which try to synchronize failed items. It run each 1 minute.
/// </summary>
internal class FailedItemsWorker : FailedSyncItemsWorkerBase<FailedItemsWorker, IDynamicsLeadsIntegrationService,
    DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}