using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Installers;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.SalesForce;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;
using Kentico.Xperience.CRM.SalesForce.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(SalesForceIntegrationGlobalEvents))]

namespace Kentico.Xperience.CRM.SalesForce;

/// <summary>
/// Module with BizFormItem and ContactInfo event handlers for SalesForce integration
/// </summary>
internal class SalesForceIntegrationGlobalEvents : Module
{
    private ILogger<SalesForceIntegrationGlobalEvents> logger = null!;
    private ICRMModuleInstaller? installer;

    public SalesForceIntegrationGlobalEvents() : base(nameof(SalesForceIntegrationGlobalEvents))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit();

        var services = parameters.Services;

        logger = services.GetRequiredService<ILogger<SalesForceIntegrationGlobalEvents>>();
        installer = services.GetRequiredService<ICRMModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;

        BizFormItemEvents.Insert.After += SynchronizeBizFormLead;
        BizFormItemEvents.Update.After += SynchronizeBizFormLead;

        ThreadWorker<FailedItemsWorker>.Current.EnsureRunningThread();

        RequestEvents.RunEndRequestTasks.Execute += (_, _) =>
        {
            FailedItemsWorker.Current.EnsureRunningThread();
        };
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        installer?.Install(CRMType.SalesForce);
    }

    private void SynchronizeBizFormLead(object? sender, BizFormItemEventArgs e)
    {
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        try
        {
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var settings = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SalesForceIntegrationSettings>>().Value;
                if (!settings.FormLeadsEnabled) return;

                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<ISalesForceLeadsIntegrationService>();

                leadsIntegrationService.SynchronizeLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
            failedSyncItemsService.LogFailedLeadItem(e.Item, CRMType.SalesForce);
        }
    }
}