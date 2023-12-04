using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Installers;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;
using Kentico.Xperience.CRM.SalesForce.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        ThreadWorker<FailedItemsWorker>.Current.EnsureRunningThread();
    }

    private async void BizFormInserted(object? sender, BizFormItemEventArgs e)
    {
        try
        {
            var settings = Service.Resolve<IOptions<SalesForceIntegrationSettings>>().Value;
            if (!settings.FormLeadsEnabled) return;

            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<ISalesForceLeadsIntegrationService>();

                await leadsIntegrationService.CreateLeadAsync(e.Item);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during inserting lead");
        }
    }

    private async void BizFormUpdated(object? sender, BizFormItemEventArgs e)
    {
        try
        {
            var settings = Service.Resolve<IOptions<SalesForceIntegrationSettings>>().Value;
            if (!settings.FormLeadsEnabled) return;

            var mappingConfig = Service.Resolve<SalesForceBizFormsMappingConfiguration>();
            if (mappingConfig.ExternalIdFieldName is not { Length: > 0 })
            {
                return;
            }

            using (var serviceScope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var leadsIntegrationService = serviceScope.ServiceProvider
                    .GetRequiredService<ISalesForceLeadsIntegrationService>();

                await leadsIntegrationService.UpdateLeadAsync(e.Item);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
        }
    }
}