using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Installers;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Salesforce;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Kentico.Xperience.CRM.Salesforce.Services;
using Kentico.Xperience.CRM.Salesforce.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(SalesforceIntegrationGlobalEvents))]

namespace Kentico.Xperience.CRM.Salesforce;

/// <summary>
/// Module with BizFormItem and ContactInfo event handlers for Salesforce integration
/// </summary>
internal class SalesforceIntegrationGlobalEvents : Module
{
    private ILogger<SalesforceIntegrationGlobalEvents> logger = null!;
    private ICRMModuleInstaller? installer;

    public SalesforceIntegrationGlobalEvents() : base(nameof(SalesforceIntegrationGlobalEvents))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit();

        var services = parameters.Services;

        logger = services.GetRequiredService<ILogger<SalesforceIntegrationGlobalEvents>>();
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
        installer?.Install(CRMType.Salesforce);
    }

    private void SynchronizeBizFormLead(object? sender, BizFormItemEventArgs e)
    {
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        try
        {
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var settings = serviceScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SalesforceIntegrationSettings>>().Value;
                if (!settings.FormLeadsEnabled) return;

                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<ISalesforceLeadsIntegrationService>();

                leadsIntegrationService.SynchronizeLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
            failedSyncItemsService.LogFailedLeadItem(e.Item, CRMType.Salesforce);
        }
    }
}