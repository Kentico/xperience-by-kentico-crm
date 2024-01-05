using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Enums;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;

namespace Kentico.Xperience.CRM.SalesForce;
public static class SalesForceServiceCollectionsExtensions
{
    /// <summary>
    /// Adds SalesForce Sales integration services for syncing BizForm items to Leads
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="formsConfig"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddSalesForceFormLeadsIntegration(this IServiceCollection serviceCollection,
        Action<BizFormsMappingBuilder> formsConfig,
        IConfiguration configuration)
    {
        serviceCollection.AddKenticoCrmCommonLeadIntegration<SalesForceBizFormsMappingConfiguration>(formsConfig);

        serviceCollection.AddOptions<SalesForceIntegrationSettings>().Bind(configuration);
        AddSalesForceCommonIntegration(serviceCollection, configuration);

        serviceCollection.AddScoped<ISalesForceLeadsIntegrationService, SalesForceLeadsIntegrationService>();
        return serviceCollection;
    }
    
    public static IServiceCollection AddSalesForceContactsIntegration(this IServiceCollection serviceCollection,
        ContactCRMType crmType, IConfiguration configuration)
        => serviceCollection.AddSalesForceContactsIntegration(crmType, b => { }, configuration);
    
    public static IServiceCollection AddSalesForceContactsIntegration(this IServiceCollection serviceCollection,
        ContactCRMType crmType,
        Action<ContactMappingBuilder> mappingConfig,
        IConfiguration configuration,
        bool useDefaultMapping = true)
    {
        serviceCollection.AddKenticoCrmCommonContactIntegration<SalesForceContactMappingConfiguration>(mappingConfig);
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

                return mappingBuilder.Build<SalesForceContactMappingConfiguration>();
            });
        
        serviceCollection.AddOptions<SalesForceIntegrationSettings>().Bind(configuration)
            .PostConfigure(s => s.ContactType = crmType);
        AddSalesForceCommonIntegration(serviceCollection, configuration);

        serviceCollection.AddScoped<ISalesForceContactsIntegrationService, SalesForceContactsIntegrationService>();
        return serviceCollection;
    }

    private static void AddSalesForceCommonIntegration(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // default cache for token management
        serviceCollection.AddDistributedMemoryCache();

        //add token management config
        serviceCollection.AddClientCredentialsTokenManagement()
            .AddClient("salesforce.api.client", client =>
            {
                var apiConfig = configuration.Get<SalesForceIntegrationSettings>()?.ApiConfig;
                
                if (apiConfig?.IsValid() is not true)
                    throw new InvalidOperationException("Missing API settings");
                
                client.TokenEndpoint = apiConfig.SalesForceUrl?.TrimEnd('/') + "/services/oauth2/token";

                client.ClientId = apiConfig.ClientId;
                client.ClientSecret = apiConfig.ClientSecret;
            });

        //add http client for salesforce api
        serviceCollection.AddHttpClient<ISalesForceApiService, SalesForceApiService>(client =>
            {
                var apiConfig = configuration.Get<SalesForceIntegrationSettings>()?.ApiConfig;
            
                if (apiConfig?.IsValid() is not true)
                    throw new InvalidOperationException("Missing API settings");

                string apiVersion = apiConfig.ApiVersion.ToString("F1", CultureInfo.InvariantCulture);
                client.BaseAddress = new Uri($"{apiConfig.SalesForceUrl?.TrimEnd('/')}/services/data/v{apiVersion}/");
            })
            .AddClientCredentialsTokenHandler("salesforce.api.client");
    }
}
