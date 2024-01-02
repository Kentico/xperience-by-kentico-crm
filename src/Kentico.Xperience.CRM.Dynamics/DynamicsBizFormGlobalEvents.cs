using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
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

[assembly: RegisterModule(typeof(DynamicsBizFormGlobalEvents))]

namespace Kentico.Xperience.CRM.Dynamics;

/// <summary>
/// Module with bizformitem event handlers for Dynamics integration
/// </summary>
internal class DynamicsBizFormGlobalEvents : Module
{
    public DynamicsBizFormGlobalEvents() : base(nameof(DynamicsBizFormGlobalEvents))
    {
    }

    private ILogger<DynamicsBizFormGlobalEvents> logger = null!;

    protected override void OnInit()
    {
        base.OnInit();

        BizFormItemEvents.Insert.After += BizFormInserted;
        BizFormItemEvents.Update.After += BizFormUpdated;
        logger = Service.Resolve<ILogger<DynamicsBizFormGlobalEvents>>();
        Service.Resolve<ICrmModuleInstaller>().Install();
        ThreadWorker<FailedItemsWorker>.Current.EnsureRunningThread();
    }

    private void BizFormInserted(object? sender, BizFormItemEventArgs e)
    {
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        try
        {
            var settings = Service.Resolve<IOptions<DynamicsIntegrationSettings>>().Value;
            if (!settings.FormLeadsEnabled) return;
            
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<IDynamicsLeadsIntegrationService>();

                leadsIntegrationService.CreateLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during inserting lead");
            failedSyncItemsService.LogFailedLeadItem(e.Item, CRMType.Dynamics);
        }
    }

    private void BizFormUpdated(object? sender, BizFormItemEventArgs e)
    {
        try
        {
            var settings = Service.Resolve<IOptions<DynamicsIntegrationSettings>>().Value;
            if (!settings.FormLeadsEnabled) return;

            var mappingConfig = Service.Resolve<DynamicsBizFormsMappingConfiguration>();
            if (mappingConfig.ExternalIdFieldName is not { Length: > 0 })
            {
                return;
            }
            
            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<IDynamicsLeadsIntegrationService>();

                leadsIntegrationService.UpdateLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
        }
    }
}