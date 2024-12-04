using System.Diagnostics;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.CRM.Common.Synchronization;

/// <summary>
/// Base class for thread workers which try to synchronize previously failed leads (biz form items)
/// Concrete implementation for each CRM must exists
/// </summary>
/// <typeparam name="TWorker"></typeparam>
/// <typeparam name="TFormLeadsService"></typeparam>
/// <typeparam name="TContactsService"></typeparam>
/// <typeparam name="TSettings"></typeparam>
/// <typeparam name="TApiConfig"></typeparam>
public abstract class
    FailedSyncItemsWorkerBase<TWorker, TFormLeadsService, TContactsService, TSettings,
        TApiConfig> : ThreadWorker<TWorker>
    where TWorker : ThreadWorker<TWorker>, new()
    where TFormLeadsService : ILeadsIntegrationService
    where TContactsService : IContactsIntegrationService
    where TSettings : CommonIntegrationSettings<TApiConfig>
    where TApiConfig : new()
{
    protected override int DefaultInterval => 60000;
    private ILogger<TWorker> logger = null!;

    protected override void Initialize()
    {
        base.Initialize();
        logger = Service.Resolve<ILogger<TWorker>>();
    }

    /// <summary>Method processing actions.</summary>
    protected override void Process()
    {
        Debug.WriteLine($"Worker {GetType().FullName} running");

        try
        {
            using var serviceScope = Service.Resolve<IServiceProvider>().CreateScope();

            var settings = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TSettings>>().Value;
            if (!settings.FormLeadsEnabled && !settings.ContactsEnabled) return;

            var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();

            ILeadsIntegrationService? leadsIntegrationService = null;
            IContactsIntegrationService? contactsIntegrationService = null;

            foreach (var syncItem in failedSyncItemsService.GetFailedSyncItemsToReSync(CRMName))
            {
                // contacts
                if (syncItem.FailedSyncItemEntityClass == ContactInfo.TYPEINFO.ObjectClassName && settings.ContactsEnabled)
                {
                    contactsIntegrationService ??= serviceScope.ServiceProvider.GetRequiredService<TContactsService>();

                    var contactInfo = ContactInfo.Provider.Get(syncItem.FailedSyncItemEntityID);
                    if (contactInfo is null)
                    {
                        syncItem.Delete();
                        continue;
                    }

                    (settings.ContactType == ContactCRMType.Lead ?
                            contactsIntegrationService.SynchronizeContactToLeadsAsync(contactInfo) :
                            contactsIntegrationService.SynchronizeContactToContactsAsync(contactInfo))
                        .GetAwaiter().GetResult();
                }
                //form submissions
                else if (settings.FormLeadsEnabled)
                {
                    leadsIntegrationService ??= serviceScope.ServiceProvider
                        .GetRequiredService<TFormLeadsService>();

                    var bizFormItem = failedSyncItemsService.GetBizFormItem(syncItem);
                    if (bizFormItem is null)
                    {
                        syncItem.Delete();
                        continue;
                    }

                    leadsIntegrationService.SynchronizeLeadAsync(bizFormItem).GetAwaiter().GetResult();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured during running '{TypeName}'", GetType().Name);
        }
    }

    /// <summary>
    /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
    /// </summary>
    protected override void Finish()
    {
    }

    protected abstract string CRMName { get; }
}