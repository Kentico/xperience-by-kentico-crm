using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Kentico.Xperience.CRM.Salesforce.Services;

namespace Kentico.Xperience.CRM.SalesForce.Workers;

public class ContactsSyncQueueWorker : ContactsSyncQueueWorkerBase<ContactsSyncQueueWorker,
    ISalesforceContactsIntegrationService, SalesforceIntegrationSettings, SalesforceApiConfig>
{
    protected override string CRMName => CRMType.Salesforce;
}