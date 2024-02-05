using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Kentico.Xperience.CRM.Salesforce.Synchronization;

namespace Kentico.Xperience.CRM.Salesforce;

internal class ContactsSyncFromCRMWorker : ContactSyncFromCRMWorkerBase<ContactsSyncFromCRMWorker,
    ISalesforceContactsIntegrationService, SalesforceIntegrationSettings, SalesforceApiConfig>
{
    protected override string CRMName => CRMType.Salesforce;
}