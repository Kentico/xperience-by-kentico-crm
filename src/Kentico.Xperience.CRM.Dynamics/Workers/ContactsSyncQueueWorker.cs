using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Workers;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kentico.Xperience.CRM.Dynamics.Workers;

internal class ContactsSyncQueueWorker : ContactsSyncQueueWorkerBase<ContactsSyncQueueWorker,
    IDynamicsContactsIntegrationService, DynamicsIntegrationSettings, DataverseApiConfig>
{
    protected override string CRMName => CRMType.Dynamics;
}