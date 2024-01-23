using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Kentico.Xperience.CRM.Dynamics;

public static class DynamicsServiceCollectionExtensions
{
    /// <summary>
    /// Adds Dynamic Sales integration services for syncing BizForm items to Leads
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="formsConfig"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddDynamicsFormLeadsIntegration(this IServiceCollection serviceCollection,
        Action<DynamicsBizFormsMappingBuilder> formsConfig,
        IConfiguration? configuration = null)
    {
        serviceCollection.AddKenticoCrmCommonFormLeadsIntegration();
        var mappingBuilder = new DynamicsBizFormsMappingBuilder(serviceCollection);
        formsConfig(mappingBuilder);
        serviceCollection.TryAddSingleton(_ => mappingBuilder.Build());

        if (configuration is null)
        {
            serviceCollection.AddOptions<DynamicsIntegrationSettings>()
                .Configure<ICRMSettingsService>(ConfigureWithCMSSettings);
        }
        else
        {
            serviceCollection.AddOptions<DynamicsIntegrationSettings>().Bind(configuration);
        }

        serviceCollection.TryAddScoped(GetCrmServiceClient);
        serviceCollection.AddScoped<IDynamicsLeadsIntegrationService, DynamicsLeadsIntegrationService>();
        return serviceCollection;
    }

    /// <summary>
    /// Create Dataverse API client
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static ServiceClient GetCrmServiceClient(IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<IOptionsSnapshot<DynamicsIntegrationSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<ServiceClient>>();

        if (!settings.ApiConfig.IsValid())
        {
            throw new InvalidOperationException("Missing API setting");
        }

        var connectionString = string.IsNullOrWhiteSpace(settings.ApiConfig.ConnectionString) ?
            $"AuthType=ClientSecret;Url={settings.ApiConfig.DynamicsUrl};ClientId={settings.ApiConfig.ClientId};ClientSecret={settings.ApiConfig.ClientSecret}" :
            settings.ApiConfig.ConnectionString;

        return new ServiceClient(connectionString, logger);
    }

    private static void ConfigureWithCMSSettings(DynamicsIntegrationSettings settings, ICRMSettingsService settingsService)
    {
        var settingsInfo = settingsService.GetSettings(CRMType.Dynamics);
        settings.FormLeadsEnabled = settingsInfo?.CRMIntegrationSettingsFormsEnabled ?? false;

        settings.IgnoreExistingRecords = settingsInfo?.CRMIntegrationSettingsIgnoreExistingRecords ?? false;

        settings.ApiConfig.DynamicsUrl = settingsInfo?.CRMIntegrationSettingsUrl;
        settings.ApiConfig.ClientId = settingsInfo?.CRMIntegrationSettingsClientId;
        settings.ApiConfig.ClientSecret = settingsInfo?.CRMIntegrationSettingsClientSecret;
    }
}