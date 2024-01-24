using Duende.AccessTokenManagement;
using Kentico.Xperience.CRM.Common;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Salesforce.Configuration;
using Kentico.Xperience.CRM.Salesforce.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Kentico.Xperience.CRM.Salesforce;

public static class SalesforceServiceCollectionsExtensions
{
    /// <summary>
    /// Adds Salesforce Sales integration services for syncing BizForm items to Leads
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="formsConfig"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddKenticoCRMSalesforce(this IServiceCollection serviceCollection,
        Action<SalesforceBizFormsMappingBuilder> formsConfig,
        IConfiguration? configuration = null)
    {
        serviceCollection.AddKenticoCrmCommonFormLeadsIntegration();

        var mappingBuilder = new SalesforceBizFormsMappingBuilder(serviceCollection);
        formsConfig(mappingBuilder);
        serviceCollection.TryAddSingleton(
            _ => mappingBuilder.Build());

        if (configuration is null)
        {
            serviceCollection.AddOptions<SalesforceIntegrationSettings>()
                .Configure<ICRMSettingsService>(ConfigureWithCMSSettings);
        }
        else
        {
            serviceCollection.AddOptions<SalesforceIntegrationSettings>().Bind(configuration);
        }

        AddSalesforceCommonIntegration(serviceCollection);

        serviceCollection.AddScoped<ISalesforceLeadsIntegrationService, SalesforceLeadsIntegrationService>();
        return serviceCollection;
    }

    private static void AddSalesforceCommonIntegration(IServiceCollection serviceCollection)
    {
        // default cache for token management
        serviceCollection.AddDistributedMemoryCache();

        //add token management config
        serviceCollection.AddClientCredentialsTokenManagement();

        serviceCollection.AddOptions<ClientCredentialsClient>("Salesforce.api.client")
            .Configure<IServiceProvider>((client, sp) =>
            {
                //cannot use IOptionsSnapshot, so changes in CMS settings needs restarting app to apply immediately
                var apiConfig = sp.GetRequiredService<IOptionsMonitor<SalesforceIntegrationSettings>>().CurrentValue
                    .ApiConfig;

                if (!apiConfig.IsValid())
                    throw new InvalidOperationException("Missing API settings");

                client.TokenEndpoint = apiConfig.SalesforceUrl?.TrimEnd('/') + "/services/oauth2/token";

                client.ClientId = apiConfig.ClientId;
                client.ClientSecret = apiConfig.ClientSecret;
            });

        //add http client for Salesforce api
        serviceCollection.AddHttpClient<ISalesforceApiService, SalesforceApiService>((provider, client) =>
            {
                //cannot use IOptionsSnapshot, so changes in CMS settings needs restarting app to apply immediately
                var settings = provider.GetRequiredService<IOptionsMonitor<SalesforceIntegrationSettings>>().CurrentValue;

                if (!settings.ApiConfig.IsValid())
                    throw new InvalidOperationException("Missing API settings");

                string apiVersion = settings.ApiConfig.ApiVersion.ToString("F1", CultureInfo.InvariantCulture);
                client.BaseAddress =
                    new Uri($"{settings.ApiConfig.SalesforceUrl?.TrimEnd('/')}/services/data/v{apiVersion}/");
            })
            .AddClientCredentialsTokenHandler("Salesforce.api.client");
    }

    private static void ConfigureWithCMSSettings(SalesforceIntegrationSettings settings, ICRMSettingsService settingsService)
    {
        var settingsInfo = settingsService.GetSettings(CRMType.Salesforce);
        settings.FormLeadsEnabled = settingsInfo?.CRMIntegrationSettingsFormsEnabled ?? false;

        settings.IgnoreExistingRecords = settingsInfo?.CRMIntegrationSettingsIgnoreExistingRecords ?? false;

        settings.ApiConfig.SalesforceUrl = settingsInfo?.CRMIntegrationSettingsUrl;
        settings.ApiConfig.ClientId = settingsInfo?.CRMIntegrationSettingsClientId;
        settings.ApiConfig.ClientSecret = settingsInfo?.CRMIntegrationSettingsClientSecret;
    }
}