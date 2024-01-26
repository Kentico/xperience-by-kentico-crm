using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Dynamics.Configuration;

namespace Kentico.Xperience.CRM.Dynamics.Synchronization;

internal class ContactsSyncFromCRMWorker : ContactSyncFromCRMWorkerBase<ContactsSyncFromCRMWorker,
    IDynamicsContactsIntegrationService, DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}