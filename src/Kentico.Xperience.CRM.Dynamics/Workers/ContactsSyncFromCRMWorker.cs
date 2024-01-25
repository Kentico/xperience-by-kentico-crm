using CMS.Base;
using CMS.Core;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kentico.Xperience.CRM.Dynamics.Workers;

public class ContactsSyncFromCRMWorker : ThreadWorker<ContactsSyncFromCRMWorker>
{
    private readonly ILogger<ContactsSyncFromCRMWorker> logger = Service.Resolve<ILogger<ContactsSyncFromCRMWorker>>();
    protected override int DefaultInterval => 60000;

    protected override void Process()
    {
        Debug.WriteLine($"Worker {GetType().FullName} running");

        try
        {
            using (var scope = Service.Resolve<IServiceProvider>().CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DynamicsIntegrationSettings>>().Value;
                if (!settings.ContactsEnabled) return;
                
                var contactsIntegrationService =
                    scope.ServiceProvider.GetRequiredService<IDynamicsContactsIntegrationService>();
                
                if (settings.ContactType == ContactCRMType.Lead)
                {
                    contactsIntegrationService.SynchronizeLeadsToKenticoAsync()
                        .GetAwaiter()
                        .GetResult();
                }

                if (settings.ContactType == ContactCRMType.Contact)
                {
                    contactsIntegrationService.SynchronizeContactsToKenticoAsync()
                        .GetAwaiter()
                        .GetResult();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured during contacts sync");
        }
    }

    protected override void Finish() { }
}