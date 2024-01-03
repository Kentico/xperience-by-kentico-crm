using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kentico.Xperience.CRM.Dynamics.Workers;

internal class ContactsSyncQueueWorker : ThreadQueueWorker<ContactInfo, ContactsSyncQueueWorker>
{
    private readonly ILogger<ContactsSyncQueueWorker> logger = Service.Resolve<ILogger<ContactsSyncQueueWorker>>();

    /// <inheritdoc />
    protected override int DefaultInterval => 10000;

    /// <inheritdoc/>
    protected override void ProcessItem(ContactInfo item)
    {
    }

    /// <inheritdoc />
    protected override int ProcessItems(IEnumerable<ContactInfo> contacts)
    {
        Debug.WriteLine($"Worker {GetType().FullName} running");
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        int processed = 0;
        
        var settings = Service.Resolve<IOptionsMonitor<DynamicsIntegrationSettings>>().CurrentValue;
        if (!settings.ContactsEnabled) return 0;

        try
        {
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var contactsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<IDynamicsContactsIntegrationService>();

                foreach (var contact in contacts)
                {
                    contactsIntegrationService.SynchronizeContactToLeadsAsync(contact).ConfigureAwait(false)
                        .GetAwaiter().GetResult();
                    processed++;
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during contacts sync");
            //@TODO
            //failedSyncItemsService.LogFailedContactItem(contactInfo, CRMType.Dynamics);
        }

        return processed;
    }

    /// <inheritdoc />
    protected override void Finish() => RunProcess();
}