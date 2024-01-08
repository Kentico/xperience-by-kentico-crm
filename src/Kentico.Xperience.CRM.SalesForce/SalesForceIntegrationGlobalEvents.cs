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

    public SalesForceIntegrationGlobalEvents() : base(nameof(SalesForceIntegrationGlobalEvents))
    {
    }

    protected override void OnInit()
    {
        base.OnInit();

        BizFormItemEvents.Insert.After += SynchronizeBizFormLead;
        BizFormItemEvents.Update.After += SynchronizeBizFormLead;
        logger = Service.Resolve<ILogger<SalesForceIntegrationGlobalEvents>>();
        Service.Resolve<ICRMModuleInstaller>().Install(CRMType.SalesForce);
        ThreadWorker<FailedItemsWorker>.Current.EnsureRunningThread();
        
        RequestEvents.RunEndRequestTasks.Execute += (_, _) =>
        {
            FailedItemsWorker.Current.EnsureRunningThread();
        };
    }
    
    private void SynchronizeBizFormLead(object? sender, BizFormItemEventArgs e)
    {
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        try
        {
            var settings = Service.Resolve<IOptionsMonitor<SalesForceIntegrationSettings>>().CurrentValue;
            if (!settings.FormLeadsEnabled) return;
            
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
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