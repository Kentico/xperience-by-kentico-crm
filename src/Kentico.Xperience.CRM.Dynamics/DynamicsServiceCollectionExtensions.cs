using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.Common.Mapping;
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
        Action<BizFormsMappingBuilder> formsConfig,
        IConfiguration configuration)
    {
        serviceCollection.AddKenticoCrmCommonLeadIntegration<DynamicsBizFormsMappingConfiguration>(formsConfig);

        serviceCollection.AddOptions<DynamicsIntegrationSettings>().Bind(configuration);
        serviceCollection.TryAddSingleton(GetCrmServiceClient);
        serviceCollection.AddScoped<IDynamicsLeadsIntegrationService, DynamicsLeadsIntegrationService>();
        return serviceCollection;
    }

    public static IServiceCollection AddDynamicsContactsIntegration(this IServiceCollection serviceCollection,
        ContactCRMType crmType, IConfiguration configuration)
    => serviceCollection.AddDynamicsContactsIntegration(crmType, b => { }, configuration);

    public static IServiceCollection AddDynamicsContactsIntegration(this IServiceCollection serviceCollection,
        ContactCRMType crmType, Action<ContactMappingBuilder> mappingConfig, IConfiguration configuration,
        bool useDefaultMapping = true)
    {
        serviceCollection.AddKenticoCrmCommonContactIntegration<DynamicsContactMappingConfiguration>(mappingConfig);
        serviceCollection.TryAddSingleton(
            sp =>
            {
                var mappingBuilder = new ContactMappingBuilder();
                if (useDefaultMapping)
                {
                    mappingBuilder = crmType == ContactCRMType.Lead ?
                        mappingBuilder.AddDefaultMappingForLead() :
                        mappingBuilder.AddDefaultMappingForContact();
                    mappingConfig(mappingBuilder);
                }

                return mappingBuilder.Build<DynamicsContactMappingConfiguration>();
            });

        serviceCollection.AddOptions<DynamicsIntegrationSettings>().Bind(configuration)
            .PostConfigure(s => s.ContactType = crmType);
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
        var settings = serviceProvider.GetRequiredService<IOptions<DynamicsIntegrationSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<ServiceClient>>();

        if (settings.ApiConfig?.IsValid() is not true)
        {
            throw new InvalidOperationException("Missing API setting");
        }

        var connectionString = string.IsNullOrWhiteSpace(settings.ApiConfig.ConnectionString) ?
            $"AuthType=ClientSecret;Url={settings.ApiConfig.DynamicsUrl};ClientId={settings.ApiConfig.ClientId};ClientSecret={settings.ApiConfig.ClientSecret}" :
            settings.ApiConfig.ConnectionString;

        return new ServiceClient(connectionString, logger);
    }
}