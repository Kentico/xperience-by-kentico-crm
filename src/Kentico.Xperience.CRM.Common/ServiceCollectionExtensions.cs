using Kentico.Xperience.CRM.Common.Admin;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;
using Kentico.Xperience.CRM.Common.Metadata;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Common.Services.Implementations;
using Kentico.Xperience.CRM.Common.Synchronization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Xperience.CRM.Common;

/// <summary>
/// Contains extension methods for CRM services registration on startup
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds common services for BizForm-Leads CRM integration. This method is usually used from specific CRM integration library
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoCrmCommonFormLeadsIntegration(this IServiceCollection services)
    {
        services.TryAddSingleton<ILeadsIntegrationValidationService, LeadIntegrationValidationService>();
        services.TryAddSingleton<ICRMModuleInstaller, CRMModuleInstaller>();
        services.TryAddSingleton<IFailedSyncItemService, FailedSyncItemService>();
        services.TryAddSingleton<ICRMSyncItemService, CRMSyncItemService>();
        services.TryAddSingleton<ICRMSettingsService, CRMSettingsService>();

        services.AddContactFieldMappingServices();

        return services;
    }

    /// <summary>
    /// Adds common services for Contacts to Leads/Contacts CRM integration. This method is usually used from specific CRM integration library 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoCrmCommonContactIntegration(this IServiceCollection services)
    {
        services.TryAddSingleton<IContactsIntegrationValidationService, ContactsIntegrationValidationService>();
        services.TryAddSingleton<ICRMModuleInstaller, CRMModuleInstaller>();
        services.TryAddSingleton<IFailedSyncItemService, FailedSyncItemService>();
        services.TryAddSingleton<ICRMSyncItemService, CRMSyncItemService>();
        services.TryAddSingleton<ICRMSettingsService, CRMSettingsService>();

        services.AddContactFieldMappingServices();

        return services;
    }

    /// <summary>
    /// Adds the services backing the visually configured contact field mappings.
    /// </summary>
    private static void AddContactFieldMappingServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IContactFieldMappingService, ContactFieldMappingService>();
        services.TryAddSingleton<IContactFieldSourceProvider, ContactFieldSourceProvider>();

        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IContactSourceValueResolver, CountryNameResolver>());
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IContactSourceValueResolver, StateNameResolver>());
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IContactSourceValueResolver, GenderLabelResolver>());
    }
}