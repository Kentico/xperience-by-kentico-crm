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

    protected override void OnInit()
    {
        base.OnInit();

        BizFormItemEvents.Insert.After += BizFormInserted;
        BizFormItemEvents.Update.After += BizFormUpdated;
        
        ContactInfo.TYPEINFO.Events.Insert.After += ContactSync;
        ContactInfo.TYPEINFO.Events.Update.After += ContactSync;
        
        logger = Service.Resolve<ILogger<DynamicsIntegrationGlobalEvents>>();
        Service.Resolve<ICrmModuleInstaller>().Install();
        RequestEvents.RunEndRequestTasks.Execute += (_, _) =>
        {
            ContactsSyncQueueWorker.Current.EnsureRunningThread();
            ContactsSyncFromCRMWorker.Current.EnsureRunningThread();
            FailedItemsWorker.Current.EnsureRunningThread();
        };
    }

    private void BizFormInserted(object? sender, BizFormItemEventArgs e)
    {
        var failedSyncItemsService = Service.Resolve<IFailedSyncItemService>();
        try
        {
            var settings = Service.Resolve<IOptionsMonitor<DynamicsIntegrationSettings>>().CurrentValue;
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
            var settings = Service.Resolve<IOptionsMonitor<DynamicsIntegrationSettings>>().CurrentValue;
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
    
    private void ContactSync(object? sender, ObjectEventArgs args)
    {
        if (args.Object is not ContactInfo contactInfo || !ValidationHelper.IsEmail(contactInfo.ContactEmail))
            return;

        ContactsSyncQueueWorker.Current.Enqueue(contactInfo);
    }
}