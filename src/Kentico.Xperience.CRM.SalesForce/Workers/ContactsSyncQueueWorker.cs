using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;

namespace Kentico.Xperience.CRM.SalesForce.Workers;

public class ContactsSyncQueueWorker : ContactsSyncQueueWorkerBase<ContactsSyncQueueWorker,
    ISalesForceContactsIntegrationService, SalesForceIntegrationSettings, SalesForceApiConfig>
{
    protected override string CRMName => CRMType.SalesForce;
}