﻿using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Kentico.Xperience.CRM.Common.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.CRM.Common.Configuration;

public abstract class ContactMappingBuilder<TBuilder>
where TBuilder : ContactMappingBuilder<TBuilder>
{
    private readonly IServiceCollection serviceCollection;
    protected readonly List<ContactFieldToCRMMapping> fieldMappings = new();
    protected readonly List<Type> converters = new();

    protected ContactMappingBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public TBuilder MapField(string contactFieldName, string crmFieldName)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldNameMapping(contactFieldName), new CRMFieldNameMapping(crmFieldName)));
        return (TBuilder)this;
    }

    public TBuilder MapField(Func<ContactInfo, object> mappingFunc, string crmFieldName)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldMappingFunction(mappingFunc), new CRMFieldNameMapping(crmFieldName)));
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds custom service for BizForm item validation before sending to CRM
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public TBuilder AddCustomValidation<TService>()
        where TService : class, IContactsIntegrationValidationService
    {
        serviceCollection.AddSingleton<IContactsIntegrationValidationService, TService>();

        return (TBuilder)this;
    }
}