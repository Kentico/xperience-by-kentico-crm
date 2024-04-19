using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Salesforce;
using Kentico.Xperience.CRM.Salesforce.Synchronization;

using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(typeof(SalesforceIntegrationGlobalEvents))]

namespace Kentico.Xperience.CRM.Salesforce;

/// <summary>
/// Module with BizFormItem and ContactInfo event handlers for Salesforce integration
/// </summary>
internal class SalesforceIntegrationGlobalEvents : Module
{
    private ICRMModuleInstaller? installer;

    public SalesforceIntegrationGlobalEvents() : base(nameof(SalesforceIntegrationGlobalEvents))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetRequiredService<ICRMModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;

        BizFormItemEvents.Insert.After += SynchronizeBizFormLead;
        BizFormItemEvents.Update.After += SynchronizeBizFormLead;

        ContactInfo.TYPEINFO.Events.Insert.After += ContactSync;
        ContactInfo.TYPEINFO.Events.Update.After += ContactSync;

        RequestEvents.RunEndRequestTasks.Execute += (_, _) =>
        {
            SalesforceSyncQueueWorker.Current.EnsureRunningThread();
            ContactsSyncFromCRMWorker.Current.EnsureRunningThread();
            FailedItemsWorker.Current.EnsureRunningThread();
        };
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        installer?.Install(CRMType.Salesforce);
    }

    private void SynchronizeBizFormLead(object? sender, BizFormItemEventArgs e)
    {
        SalesforceSyncQueueWorker.Current.Enqueue(e.Item);
    }

    private void ContactSync(object? sender, ObjectEventArgs args)
    {
        if (args.Object is not ContactInfo contactInfo ||
            ValidationHelper.GetBoolean(RequestStockHelper.GetItem("SuppressEvents"), false) ||
            !ValidationHelper.IsEmail(contactInfo.ContactEmail))
            return;

        SalesforceSyncQueueWorker.Current.Enqueue(contactInfo);
    }
}