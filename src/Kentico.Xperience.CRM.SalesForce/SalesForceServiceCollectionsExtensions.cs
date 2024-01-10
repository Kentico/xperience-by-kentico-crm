using CMS.Core;
using CMS.Helpers;
using Duende.AccessTokenManagement;
using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.SalesForce.Configuration;
using Kentico.Xperience.CRM.SalesForce.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        Action<SalesForceBizFormsMappingBuilder> formsConfig,
        IConfiguration configuration)
    {
        serviceCollection.AddKenticoCrmCommonFormLeadsIntegration();
        serviceCollection.TryAddSingleton(
            _ =>
            {
                var mappingBuilder = new SalesForceBizFormsMappingBuilder(serviceCollection);
                formsConfig(mappingBuilder);
                return mappingBuilder.Build();
            });

        serviceCollection.AddOptions<SalesForceIntegrationSettings>().Bind(configuration)
            .PostConfigure<ISettingsService>(ConfigureWithCMSSettings);

        AddSalesForceCommonIntegration(serviceCollection, configuration);

        serviceCollection.AddScoped<ISalesForceLeadsIntegrationService, SalesForceLeadsIntegrationService>();
        return serviceCollection;
    }

    private static void AddSalesForceCommonIntegration(IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // default cache for token management
        serviceCollection.AddDistributedMemoryCache();

        //add token management config
        serviceCollection.AddClientCredentialsTokenManagement();

        serviceCollection.AddOptions<ClientCredentialsClient>("salesforce.api.client")
            .Configure<IServiceProvider>((client, sp) =>
            {
                //cannot use IOptionsSnapshot, so changes in CMS settings needs restarting app to apply immediately
                var apiConfig = sp.GetRequiredService<IOptionsMonitor<SalesForceIntegrationSettings>>().CurrentValue
                    .ApiConfig;

                if (!apiConfig.IsValid())
                    throw new InvalidOperationException("Missing API settings");

                client.TokenEndpoint = apiConfig.SalesForceUrl?.TrimEnd('/') + "/services/oauth2/token";

                client.ClientId = apiConfig.ClientId;
                client.ClientSecret = apiConfig.ClientSecret;
            });

        //add http client for salesforce api
        serviceCollection.AddHttpClient<ISalesForceApiService, SalesForceApiService>((provider, client) =>
            {
                //cannot use IOptionsSnapshot, so changes in CMS settings needs restarting app to apply immediately
                var settings = provider.GetRequiredService<IOptionsMonitor<SalesForceIntegrationSettings>>().CurrentValue;

                if (!settings.ApiConfig.IsValid())
                    throw new InvalidOperationException("Missing API settings");

                string apiVersion = settings.ApiConfig.ApiVersion.ToString("F1", CultureInfo.InvariantCulture);
                client.BaseAddress =
                    new Uri($"{settings.ApiConfig.SalesForceUrl?.TrimEnd('/')}/services/data/v{apiVersion}/");
            })
            .AddClientCredentialsTokenHandler("salesforce.api.client");
    }

    private static void ConfigureWithCMSSettings(SalesForceIntegrationSettings settings,
        ISettingsService settingsService)
    {
        var formsEnabled = settingsService[SettingKeys.SalesForceFormLeadsEnabled];
        if (!string.IsNullOrWhiteSpace(formsEnabled))
        {
            settings.FormLeadsEnabled = ValidationHelper.GetBoolean(formsEnabled, false);
        }

        var salesForceUrl = settingsService[SettingKeys.SalesForceUrl];
        if (!string.IsNullOrWhiteSpace(salesForceUrl))
        {
            settings.ApiConfig.SalesForceUrl = salesForceUrl;
        }

        var clientId = settingsService[SettingKeys.SalesForceClientId];
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            settings.ApiConfig.ClientId = clientId;
        }

        var clientSecret = settingsService[SettingKeys.SalesForceClientSecret];
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            settings.ApiConfig.ClientSecret = clientSecret;
        }
    }
}