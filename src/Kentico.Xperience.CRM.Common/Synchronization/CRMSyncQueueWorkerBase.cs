using System.Diagnostics;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Synchronization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.CRM.Common.Workers;

/// <summary>
/// Base class for contacts synchronization from CRM to Kentico
/// </summary>
/// <typeparam name="TWorker"></typeparam>
/// <typeparam name="TFormLeadsService"></typeparam>
/// <typeparam name="TContactsService"></typeparam>
/// <typeparam name="TSettings"></typeparam>
/// <typeparam name="TApiConfig"></typeparam>
public abstract class
    CRMSyncQueueWorkerBase<TWorker, TFormLeadsService, TContactsService, TSettings,
        TApiConfig> : ThreadQueueWorker<BaseInfo, TWorker>
    where TWorker : ThreadQueueWorker<BaseInfo, TWorker>, new()
    where TFormLeadsService : ILeadsIntegrationService
    where TContactsService : IContactsIntegrationService
    where TSettings : CommonIntegrationSettings<TApiConfig>
    where TApiConfig : new()
{
    private readonly ILogger<TWorker> logger = Service.Resolve<ILogger<TWorker>>();

    /// <inheritdoc />
    protected override int DefaultInterval => 10000;

    /// <inheritdoc/>
    protected override void ProcessItem(BaseInfo item)
    {
    }

    /// <inheritdoc />
    protected override int ProcessItems(IEnumerable<BaseInfo> items)
    {
        Debug.WriteLine($"Worker {GetType().FullName} running");

        var itemsList = items.ToList();
        var formItems = itemsList.OfType<BizFormItem>().ToList();
        var contactList = itemsList.OfType<ContactInfo>().ToList();

        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();

        using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
        {
            var settings = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TSettings>>().Value;

            if (settings.FormLeadsEnabled && formItems.Any())
            {
                try
                {
                    var leadsIntegrationService = serviceScope.ServiceProvider
                        .GetRequiredService<TFormLeadsService>();

                    foreach (var formItem in formItems)
                    {
                        leadsIntegrationService.SynchronizeLeadAsync(formItem).GetAwaiter().GetResult();
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error occured during updating lead");
                    failedSyncItemsService.LogFailedLeadItems(formItems, CRMName);
                }
            }

            if (settings.ContactsEnabled && contactList.Any())
            {
                try
                {
                    var contactsIntegrationService = serviceScope.ServiceProvider
                        .GetRequiredService<TContactsService>();

                    foreach (var contactInfo in contactList)
                    {
                        (settings.ContactType == ContactCRMType.Lead ?
                                contactsIntegrationService.SynchronizeContactToLeadsAsync(contactInfo) :
                                contactsIntegrationService.SynchronizeContactToContactsAsync(contactInfo))
                            .GetAwaiter().GetResult();
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Error occured during contacts sync");
                    failedSyncItemsService.LogFailedContactItems(contactList, CRMName);
                }
            }
        }

        return formItems.Count + contactList.Count;
    }

    /// <inheritdoc />
    protected override void Finish() => RunProcess();

    protected abstract string CRMName { get; }
}