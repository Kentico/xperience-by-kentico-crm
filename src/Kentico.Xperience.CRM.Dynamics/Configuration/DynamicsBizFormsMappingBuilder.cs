using CMS.Core;
using CMS.OnlineForms;
using CMS.OnlineForms.Internal;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Dynamics.Converters;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

public class DynamicsBizFormsMappingBuilder
{
    private readonly IServiceCollection serviceCollection;
    protected readonly Dictionary<string, BizFormFieldsMappingBuilder> forms = new();
    protected readonly Dictionary<string, List<Type>> converters = new();
    
    public DynamicsBizFormsMappingBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public DynamicsBizFormsMappingBuilder AddForm(string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));
        
        return this;
    }

    public DynamicsBizFormsMappingBuilder AddFormWithContactMapping(string formCodeName) 
        => AddFormWithContactMapping(formCodeName, b => b);

    public DynamicsBizFormsMappingBuilder AddFormWithContactMapping(
        string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));

        AddFormWithConverter<FormContactMappingToLeadConverter>(formCodeName);
        return this;
    }

    public DynamicsBizFormsMappingBuilder AddFormWithConverter<TConverter>(string formCodeName)
        where TConverter : class, ICRMTypeConverter<BizFormItem, Lead>
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        
        if (converters.TryGetValue(formCodeName.ToLowerInvariant(), out var values))
        {
            values.Add(typeof(FormContactMappingToLeadConverter));
        }
        else
        {
            converters[formCodeName.ToLowerInvariant()] = new List<Type> { typeof(TConverter)};
        }
        
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<ICRMTypeConverter<BizFormItem, Lead>, TConverter>());
        return this;
    }
    
    /// <summary>
    /// Adds custom service for BizForm item validation before sending to CRM
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public DynamicsBizFormsMappingBuilder AddCustomValidation<TService>()
        where TService : class, ILeadsIntegrationValidationService
    {
        serviceCollection.AddSingleton<ILeadsIntegrationValidationService, TService>();

        return this;
    }
    
    internal DynamicsBizFormsMappingConfiguration Build()
    {
        return new DynamicsBizFormsMappingConfiguration
        {
            FormsMappings = forms.Select(f => (f.Key, f.Value.Build()))
                .ToDictionary(r => r.Key, r => r.Item2),
            FormsConverters = converters
        };
    }
}