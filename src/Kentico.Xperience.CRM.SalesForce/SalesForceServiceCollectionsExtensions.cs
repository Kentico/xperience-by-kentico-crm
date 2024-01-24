using Duende.AccessTokenManagement;
using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Services;
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
    public static IServiceCollection AddKenticoCRMSalesForce(this IServiceCollection serviceCollection,
        Action<SalesForceBizFormsMappingBuilder> formsConfig,
        IConfiguration? configuration = null)
    {
        serviceCollection.AddKenticoCrmCommonFormLeadsIntegration();

        var mappingBuilder = new SalesForceBizFormsMappingBuilder(serviceCollection);
        formsConfig(mappingBuilder);
        serviceCollection.TryAddSingleton(
            _ => mappingBuilder.Build());

        if (configuration is null)
        {
            serviceCollection.AddOptions<SalesForceIntegrationSettings>()
                .Configure<ICRMSettingsService>(ConfigureWithCMSSettings);
        }
        else
        {
            serviceCollection.AddOptions<SalesForceIntegrationSettings>().Bind(configuration);
        }

        AddSalesForceCommonIntegration(serviceCollection);

        serviceCollection.AddScoped<ISalesForceLeadsIntegrationService, SalesForceLeadsIntegrationService>();
        return serviceCollection;
    }

    private static void AddSalesForceCommonIntegration(IServiceCollection serviceCollection)
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

    private static void ConfigureWithCMSSettings(SalesForceIntegrationSettings settings, ICRMSettingsService settingsService)
    {
        var settingsInfo = settingsService.GetSettings(CRMType.SalesForce);
        settings.FormLeadsEnabled = settingsInfo?.CRMIntegrationSettingsFormsEnabled ?? false;

        settings.IgnoreExistingRecords = settingsInfo?.CRMIntegrationSettingsIgnoreExistingRecords ?? false;

        settings.ApiConfig.SalesForceUrl = settingsInfo?.CRMIntegrationSettingsUrl;
        settings.ApiConfig.ClientId = settingsInfo?.CRMIntegrationSettingsClientId;
        settings.ApiConfig.ClientSecret = settingsInfo?.CRMIntegrationSettingsClientSecret;
    }
}