﻿using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Configuration;
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
    public static IServiceCollection AddDynamicsCrmLeadsIntegration(this IServiceCollection serviceCollection,
        Action<BizFormsMappingBuilder> formsConfig,
        IConfiguration configuration)
    {
        serviceCollection.AddKenticoCrmCommonLeadIntegration<DynamicsBizFormsMappingConfiguration>(formsConfig);

        serviceCollection.AddOptions<DynamicsIntegrationSettings>().Bind(configuration);
        serviceCollection.TryAddSingleton(GetCrmServiceClient);
        serviceCollection.AddScoped<IDynamicsLeadsIntegrationService, DynamicsLeadsIntegrationService>();
        return serviceCollection;
    }

    public static IServiceCollection AddDynamicsCrmContactsToLeadsIntegration(this IServiceCollection serviceCollection,
        Action<ContactMappingBuilder> contactsConfig, IConfiguration configuration)
    {
        serviceCollection.AddKenticoCrmCommonContactIntegration<DynamicsContactMappingConfiguration>(contactsConfig);
        serviceCollection.TryAddSingleton(
            _ =>
            {
                var mappingBuilder = new ContactMappingBuilder();
                mappingBuilder.AddDefaultMappingForLead();
                contactsConfig(mappingBuilder);
                return mappingBuilder.Build<DynamicsContactMappingConfiguration>();
            });

        serviceCollection.AddOptions<DynamicsIntegrationSettings>().Bind(configuration);
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
        var logger = serviceProvider.GetRequiredService<ILogger<DynamicsLeadsIntegrationService>>();

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