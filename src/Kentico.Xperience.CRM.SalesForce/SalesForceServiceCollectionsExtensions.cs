using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddSalesForceCrmLeadsIntegration(this IServiceCollection serviceCollection,
        Action<BizFormsMappingBuilder> formsConfig,
        IConfiguration configuration)
    {
        serviceCollection.AddKenticoCrmCommonIntegration<SalesForceBizFormsMappingConfiguration>(formsConfig);
        serviceCollection.AddOptions<SalesForceIntegrationSettings>().Bind(configuration);

        // default cache for token management
        serviceCollection.AddDistributedMemoryCache();

        //add token management config
        serviceCollection.AddClientCredentialsTokenManagement()
            .AddClient("salesforce.api.client", client =>
            {
                var config = configuration.Get<SalesForceIntegrationSettings>();

                if (config?.ApiConfig is not { SalesForceUrl: string, ClientId: string, ClientSecret: string } apiConfig)
                    throw new InvalidOperationException("Missing API settings");

                client.TokenEndpoint = apiConfig.SalesForceUrl.TrimEnd('/') + "/services/oauth2/token";

                client.ClientId = apiConfig.ClientId;
                client.ClientSecret = apiConfig.ClientSecret;
            });

        //add http client for salesforce api
        serviceCollection.AddHttpClient<ISalesForceApiService, SalesForceApiService>(client =>
        {
            var config = configuration.Get<SalesForceIntegrationSettings>();
            if (config?.ApiConfig is not { SalesForceUrl: string, ClientId: string, ClientSecret: string } apiConfig)
                throw new InvalidOperationException("Missing API settings");

            client.BaseAddress = new Uri($"{apiConfig.SalesForceUrl.TrimEnd('/')}/services/data/v59.0/");
        })
        .AddClientCredentialsTokenHandler("salesforce.api.client");


        serviceCollection.AddScoped<ISalesForceLeadsIntegrationService, SalesForceLeadsIntegrationService>();
        return serviceCollection;
    }
}
