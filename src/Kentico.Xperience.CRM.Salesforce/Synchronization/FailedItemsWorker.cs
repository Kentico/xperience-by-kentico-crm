using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Salesforce.Configuration;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

/// <summary>
/// Specific thread worker for Salesforce which try to synchronize failed items. It run each 1 minute.
/// </summary>
internal class FailedItemsWorker : FailedSyncItemsWorkerBase<FailedItemsWorker, ISalesforceLeadsIntegrationService,
    ISalesforceContactsIntegrationService, SalesforceIntegrationSettings, SalesforceApiConfig>
{
    protected override string CRMName => CRMType.Salesforce;
}