using CMS.Base;
using CMS.Core;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.SalesForce.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Kentico.Xperience.CRM.SalesForce.Workers;

public class FailedItemsWorker : ThreadWorker<FailedItemsWorker>
{
    protected override int DefaultInterval => 10000;
    private ILogger<FailedItemsWorker> logger = null!;
    
    protected override void Initialize()
    {
        base.Initialize();
        logger = Service.Resolve<ILogger<FailedItemsWorker>>();
    }

    /// <summary>Method processing actions.</summary>
    protected override void Process()
    {
        Debug.WriteLine($"Worker {this.GetType().Name} running");

        try
        {
            using var serviceScope = Service.Resolve<IServiceProvider>().CreateScope();
            
            var leadsIntegrationService = serviceScope.ServiceProvider
                .GetRequiredService<ISalesForceLeadsIntegrationService>();
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error occured during running '{nameof(FailedItemsWorker)}'");
        }
    }

    /// <summary>
    /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
    /// </summary>
    protected override void Finish()
    {
    }
}