using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;

namespace Kentico.Xperience.CRM.SalesForce.Workers;

/// <summary>
/// Specific thread worker for SalesForce which try to synchronize failed items. It run each 1 minute.
/// </summary>
internal class FailedItemsWorker : FailedSyncItemsWorkerBase<FailedItemsWorker, ISalesForceLeadsIntegrationService,
    SalesForceIntegrationSettings, SalesForceApiConfig>
{
    protected override string CRMName => CRMType.SalesForce;
}