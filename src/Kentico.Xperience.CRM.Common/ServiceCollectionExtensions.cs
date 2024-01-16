using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Installers;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Xperience.CRM.Common;

/// <summary>
/// Contains extension methods for CRM services registration on startup
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds common services for CRM integration. This method is usually used from specific CRM integration library
    /// </summary>
    /// <param name="services"></param>
    /// <param name="formsMappingConfig"></param>
    /// <typeparam name="TMappingConfiguration"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddKenticoCrmCommonFormLeadsIntegration(
        this IServiceCollection services)
    {
        services.TryAddSingleton<ILeadsIntegrationValidationService, LeadIntegrationValidationService>();

        services.TryAddSingleton<ICRMModuleInstaller, CRMModuleInstaller>();
        services.TryAddSingleton<IFailedSyncItemService, FailedSyncItemService>();
        services.TryAddSingleton<ICRMSyncItemService, CRMSyncItemService>();

        return services;
    }

    public static IServiceCollection AddKenticoCrmCommonContactIntegration<TMappingConfiguration>(
        this IServiceCollection services, Action<ContactMappingBuilder> contactMappingConfig)
        where TMappingConfiguration : ContactMappingConfiguration, new()
    {
        services.TryAddSingleton<IContactsIntegrationValidationService, ContactsIntegrationValidationService>();
        services.TryAddSingleton<ICRMModuleInstaller, CRMModuleInstaller>();
        services.TryAddSingleton<IFailedSyncItemService, FailedSyncItemService>();

        return services;
    }
}