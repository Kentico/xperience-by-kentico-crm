using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Salesforce.Configuration;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

internal class SalesforceSyncQueueWorker : CRMSyncQueueWorkerBase<SalesforceSyncQueueWorker,
    ISalesforceLeadsIntegrationService, ISalesforceContactsIntegrationService, SalesforceIntegrationSettings,
    SalesforceApiConfig>
{
    protected override string CRMName => CRMType.Salesforce;
}