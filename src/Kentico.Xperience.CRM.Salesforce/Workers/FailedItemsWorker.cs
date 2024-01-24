using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Kentico.Xperience.CRM.Salesforce.Services;

namespace Kentico.Xperience.CRM.Salesforce.Workers;

/// <summary>
/// Specific thread worker for Salesforce which try to synchronize failed items. It run each 1 minute.
/// </summary>
internal class FailedItemsWorker : FailedSyncItemsWorkerBase<FailedItemsWorker, ISalesforceLeadsIntegrationService,
    SalesforceIntegrationSettings, SalesforceApiConfig>
{
    protected override string CRMName => CRMType.Salesforce;
}