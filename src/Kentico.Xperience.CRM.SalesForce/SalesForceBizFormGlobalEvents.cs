﻿using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.CRM.SalesForce;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;
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
    }

    private void BizFormInserted(object? sender, BizFormItemEventArgs e)
    {
        try
        {
            var settings = Service.Resolve<IOptions<SalesForceIntegrationSettings>>().Value;
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
        }
    }

    private void BizFormUpdated(object? sender, BizFormItemEventArgs e)
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

                leadsIntegrationService.UpdateLeadAsync(e.Item).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured during updating lead");
        }
    }
}