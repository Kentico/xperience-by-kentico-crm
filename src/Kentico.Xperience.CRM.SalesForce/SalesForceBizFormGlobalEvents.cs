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

[assembly: RegisterModule(typeof(SalesForceBizFormGlobalEvents))]

namespace Kentico.Xperience.CRM.SalesForce;

/// <summary>
/// Module with bizformitem event handlers for SalesForce Sales integration
/// </summary>
internal class SalesForceBizFormGlobalEvents : Module
{
    private ILogger<SalesForceBizFormGlobalEvents> logger = null!;

    public SalesForceBizFormGlobalEvents() : base(nameof(SalesForceBizFormGlobalEvents))
    {
    }

    protected override void OnInit()
    {
        base.OnInit();

        BizFormItemEvents.Insert.After += BizFormInserted;
        BizFormItemEvents.Update.After += BizFormUpdated;
        logger = Service.Resolve<ILogger<SalesForceBizFormGlobalEvents>>();
        Service.Resolve<ICrmModuleInstaller>().Install();
        RequestEvents.RunEndRequestTasks.Execute += (_, _) => FailedItemsWorker.Current.EnsureRunningThread();
    }

    private void BizFormInserted(object? sender, BizFormItemEventArgs e)
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

                leadsIntegrationService.CreateLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during inserting lead");
            failedSyncItemsService.LogFailedLeadItem(e.Item, CRMType.SalesForce);
        }
    }

    private void BizFormUpdated(object? sender, BizFormItemEventArgs e)
    {
        try
        {
            var settings = Service.Resolve<IOptionsMonitor<SalesForceIntegrationSettings>>().CurrentValue;
            if (!settings.FormLeadsEnabled) return;

            var mappingConfig = Service.Resolve<SalesForceBizFormsMappingConfiguration>();
            if (string.IsNullOrWhiteSpace(mappingConfig.ExternalIdFieldName))
            {
                return;
            }

            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<ISalesForceLeadsIntegrationService>();

                leadsIntegrationService.UpdateLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
        }
    }
}