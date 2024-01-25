using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Installers;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;
using Kentico.Xperience.CRM.Dynamics.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(DynamicsIntegrationGlobalEvents))]

namespace Kentico.Xperience.CRM.Dynamics;

/// <summary>
/// Module with BizFormItem and ContactInfo event handlers for Dynamics integration
/// </summary>
internal class DynamicsIntegrationGlobalEvents : Module
{
    public DynamicsIntegrationGlobalEvents() : base(nameof(DynamicsIntegrationGlobalEvents))
    {
    }

    private ILogger<DynamicsIntegrationGlobalEvents> logger = null!;
    private ICRMModuleInstaller? installer;

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        logger = services.GetRequiredService<ILogger<DynamicsIntegrationGlobalEvents>>();
        installer = services.GetRequiredService<ICRMModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;

        BizFormItemEvents.Insert.After += SynchronizeBizFormLead;
        BizFormItemEvents.Update.After += SynchronizeBizFormLead;
        ContactInfo.TYPEINFO.Events.Insert.After += ContactSync;
        ContactInfo.TYPEINFO.Events.Update.After += ContactSync;

        logger = Service.Resolve<ILogger<DynamicsIntegrationGlobalEvents>>();
        Service.Resolve<ICRMModuleInstaller>().Install(CRMType.Dynamics);

        RequestEvents.RunEndRequestTasks.Execute += (_, _) =>
        {
            ContactsSyncQueueWorker.Current.EnsureRunningThread();
            ContactsSyncFromCRMWorker.Current.EnsureRunningThread();
            FailedItemsWorker.Current.EnsureRunningThread();
        };
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        installer?.Install(CRMType.Dynamics);
    }
    
    private void SynchronizeBizFormLead(object? sender, BizFormItemEventArgs e)
    {
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        try
        {
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var settings = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DynamicsIntegrationSettings>>().Value;
                if (!settings.FormLeadsEnabled) return;

                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<IDynamicsLeadsIntegrationService>();

                leadsIntegrationService.SynchronizeLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
            failedSyncItemsService.LogFailedLeadItem(e.Item, CRMType.Dynamics);
        }
    }

    private void ContactSync(object? sender, ObjectEventArgs args)
    {
        if (args.Object is not ContactInfo contactInfo || !ValidationHelper.IsEmail(contactInfo.ContactEmail))
            return;
        
        ContactsSyncQueueWorker.Current.Enqueue(contactInfo);
    }
}