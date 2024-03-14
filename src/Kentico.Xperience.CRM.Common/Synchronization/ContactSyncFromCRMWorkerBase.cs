using CMS.Base;
using CMS.Core;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kentico.Xperience.CRM.Common.Synchronization;

public abstract class
    ContactSyncFromCRMWorkerBase<TWorker, TContactsService, TSettings, TApiConfig> : ThreadWorker<TWorker>
    where TWorker : ThreadWorker<TWorker>, new()
    where TContactsService : IContactsIntegrationService
    where TSettings : CommonIntegrationSettings<TApiConfig>
    where TApiConfig : new()
{
    protected override int DefaultInterval => 60000;
    private ILogger<TWorker> logger = null!;
    private IContactsLastSyncInfoProvider lastSyncInfoProvider = null!;

    protected abstract string CRMName { get; }

    protected override void Initialize()
    {
        base.Initialize();
        logger = Service.Resolve<ILogger<TWorker>>();
        lastSyncInfoProvider = Service.Resolve<IContactsLastSyncInfoProvider>();
    }

    protected override void Process()
    {
        Debug.WriteLine($"Worker {GetType().FullName} running");

        try
        {
            using (var scope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TSettings>>().Value;
                if (!settings.ContactsEnabled || !settings.ContactsTwoWaySyncEnabled) return;

                var contactsIntegrationService =
                    scope.ServiceProvider.GetRequiredService<TContactsService>();

                var lastSync = GetLastSyncInfo();
                var lastSyncTime = lastSync?.ContactsLastSyncTime ?? DateTime.Now.AddMinutes(-1);
                var dateBeforeSync = DateTime.Now;
                
                (settings.ContactType == ContactCRMType.Lead
                        ? contactsIntegrationService.SynchronizeLeadsToKenticoAsync(lastSyncTime)
                        : contactsIntegrationService.SynchronizeContactsToKenticoAsync(lastSyncTime))
                    .GetAwaiter().GetResult();

                (lastSync ??= new ContactsLastSyncInfo { ContactsLastSyncCRM = CRMName }).ContactsLastSyncTime =
                    dateBeforeSync;
                lastSyncInfoProvider.Set(lastSync);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured during contacts sync");
        }
    }

    private ContactsLastSyncInfo? GetLastSyncInfo() => lastSyncInfoProvider.Get()
        .WhereEquals(nameof(ContactsLastSyncInfo.ContactsLastSyncCRM), CRMName)
        .TopN(1)
        .FirstOrDefault();
    
    protected override void Finish() { }
}