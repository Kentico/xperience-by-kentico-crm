using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Dynamics.Configuration;
using Kentico.Xperience.CRM.Dynamics.Synchronization;

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
    public static IServiceCollection AddKenticoCRMDynamics(this IServiceCollection serviceCollection,
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

    public static IServiceCollection AddKenticoCRMDynamicsContactsIntegration(
        this IServiceCollection serviceCollection,
        ContactCRMType crmType,
        IConfiguration? configuration = null,
        bool useDefaultMappingToCRM = true,
        bool useDefaultMappingToKentico = true)
        => serviceCollection.AddKenticoCRMDynamicsContactsIntegration(crmType, b => { }, configuration,
            useDefaultMappingToCRM, useDefaultMappingToKentico);

    public static IServiceCollection AddKenticoCRMDynamicsContactsIntegration(
        this IServiceCollection serviceCollection,
        ContactCRMType crmType,
        Action<DynamicsContactMappingBuilder> mappingConfig,
        IConfiguration? configuration = null,
        bool useDefaultMappingToCRM = true,
        bool useDefaultMappingToKentico = true)
    {
        serviceCollection.AddKenticoCrmCommonContactIntegration();

        var mappingBuilder = new DynamicsContactMappingBuilder(serviceCollection);
        if (useDefaultMappingToCRM)
        {
            mappingBuilder = crmType == ContactCRMType.Lead ?
                mappingBuilder.AddDefaultMappingForLead() :
                mappingBuilder.AddDefaultMappingForContact();
        }
        mappingConfig(mappingBuilder);

        if (useDefaultMappingToKentico)
        {
            mappingBuilder.AddDefaultMappingToKenticoContact();
        }

        serviceCollection.TryAddSingleton(
            _ => mappingBuilder.Build());

        if (configuration is null)
        {
            serviceCollection.AddOptions<DynamicsIntegrationSettings>()
                .Configure<ICRMSettingsService>(ConfigureWithCMSSettings)
                .PostConfigure(s => s.ContactType = crmType);
        }
        else
        {
            serviceCollection.AddOptions<DynamicsIntegrationSettings>().Bind(configuration)
                .PostConfigure(s => s.ContactType = crmType);
        }

        serviceCollection.TryAddSingleton(GetCrmServiceClient);
        serviceCollection.AddScoped<IDynamicsContactsIntegrationService, DynamicsContactsIntegrationService>();

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

    private static void ConfigureWithCMSSettings(DynamicsIntegrationSettings settings,
        ICRMSettingsService settingsService)
    {
        var settingsInfo = settingsService.GetSettings(CRMType.Dynamics);
        settings.FormLeadsEnabled = settingsInfo?.CRMIntegrationSettingsFormsEnabled ?? false;
        settings.ContactsEnabled = settingsInfo?.CRMIntegrationSettingsContactsEnabled ?? false;
        settings.ContactsTwoWaySyncEnabled = settingsInfo?.CRMIntegrationSettingsContactsTwoWaySyncEnabled ?? true;

        settings.IgnoreExistingRecords = settingsInfo?.CRMIntegrationSettingsIgnoreExistingRecords ?? false;

        settings.ApiConfig.DynamicsUrl = settingsInfo?.CRMIntegrationSettingsUrl;
        settings.ApiConfig.ClientId = settingsInfo?.CRMIntegrationSettingsClientId;
        settings.ApiConfig.ClientSecret = settingsInfo?.CRMIntegrationSettingsClientSecret;
    }
}