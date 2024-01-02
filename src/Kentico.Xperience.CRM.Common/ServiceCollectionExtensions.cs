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
    public static IServiceCollection AddKenticoCrmCommonLeadIntegration<TMappingConfiguration>(
        this IServiceCollection services, Action<BizFormsMappingBuilder> formsMappingConfig)
        where TMappingConfiguration : BizFormsMappingConfiguration, new()
    {
        services.TryAddSingleton<ILeadsIntegrationValidationService, LeadIntegrationValidationService>();

        services.TryAddSingleton(
            _ =>
            {
                var mappingBuilder = new BizFormsMappingBuilder();
                formsMappingConfig(mappingBuilder);
                return mappingBuilder.Build<TMappingConfiguration>();
            });
        
        services.TryAddSingleton<ICrmModuleInstaller, CrmModuleInstaller>();
        services.TryAddSingleton<IFailedSyncItemService, FailedSyncItemService>();

        return services;
    }

    public static IServiceCollection AddKenticoCrmCommonContactIntegration<TMappingConfiguration>(
        this IServiceCollection services, Action<ContactMappingBuilder> contactMappingConfig)
        where TMappingConfiguration : ContactMappingConfiguration, new()
    {
        services.TryAddSingleton(
            _ =>
            {
                var mappingBuilder = new ContactMappingBuilder();
                contactMappingConfig(mappingBuilder);
                return mappingBuilder.Build<TMappingConfiguration>();
            });
        
        services.TryAddSingleton<ICrmModuleInstaller, CrmModuleInstaller>();
        services.TryAddSingleton<IFailedSyncItemService, FailedSyncItemService>();

        return services;
    }
    
    /// <summary>
    /// Adds custom service for BizForm item validation before sending to CRM
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddCustomFormLeadsValidationService<TService>(this IServiceCollection services)
        where TService : class, ILeadsIntegrationValidationService
    {
        services.AddSingleton<ILeadsIntegrationValidationService, TService>();

        return services;
    }
}
