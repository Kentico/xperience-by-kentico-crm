using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Synchronization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kentico.Xperience.CRM.Common.Workers;

public abstract class ContactsSyncQueueWorkerBase<TWorker, TService, TSettings, TApiConfig> : ThreadQueueWorker<ContactInfo, TWorker>
    where TWorker : ThreadQueueWorker<ContactInfo, TWorker>, new()
    where TService : IContactsIntegrationService
    where TSettings : CommonIntegrationSettings<TApiConfig>
    where TApiConfig : new()
{
    private readonly ILogger<TWorker> logger = Service.Resolve<ILogger<TWorker>>();

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
        
        try
        {
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var contactList = contacts.ToList();
                var settings = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TSettings>>().Value;
                if (!settings.ContactsEnabled || !contactList.Any()) return 0;
                
                var contactsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<TService>();

                foreach (var contact in contactList)
                {
                    if (settings.ContactType == ContactCRMType.Lead)
                    {
                        contactsIntegrationService.SynchronizeContactToLeadsAsync(contact).ConfigureAwait(false)
                            .GetAwaiter().GetResult();
                    }
                    else
                    {
                        contactsIntegrationService.SynchronizeContactToContactsAsync(contact).ConfigureAwait(false)
                            .GetAwaiter().GetResult();
                    }
                    processed++;
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during contacts sync");
            //@TODO
            //failedSyncItemsService.LogFailedContactItem(contactInfo, CRMName);
        }

        return processed;
    }

    /// <inheritdoc />
    protected override void Finish() => RunProcess();
    
    protected abstract string CRMName { get; }
}