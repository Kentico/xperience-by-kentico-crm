using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;
using Kentico.Xperience.CRM.Common.Workers;

namespace Kentico.Xperience.CRM.Dynamics.Workers;

public class FailedItemsWorker : FailedSyncItemsWorkerBase<FailedItemsWorker, IDynamicsLeadsIntegrationService,
    DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}