using CMS.OnlineForms;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Synchronization;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

/// <summary>
/// Mapping builder for BizForm to Leads mapping
/// </summary>
public class DynamicsBizFormsMappingBuilder
{
    private readonly IServiceCollection serviceCollection;
    protected readonly Dictionary<string, BizFormFieldsMappingBuilder> forms = new();
    protected readonly Dictionary<string, List<Type>> converters = new();

    public DynamicsBizFormsMappingBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    /// <summary>
    /// Adds Form with mapping
    /// </summary>
    /// <param name="formCodeName"></param>
    /// <param name="configureFields"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DynamicsBizFormsMappingBuilder AddForm(string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));

        return this;
    }

    /// <summary>
    /// Adds form when conversion is added automatically based on Form-Contact mapping <see cref="FormContactMappingToLeadConverter"/>
    /// </summary>
    /// <param name="formCodeName"></param>
    /// <returns></returns>
    public DynamicsBizFormsMappingBuilder AddFormWithContactMapping(string formCodeName)
        => AddFormWithContactMapping(formCodeName, b => b);

    /// <summary>
    /// Adds form when conversion is added automatically based on Form-Contact mapping <see cref="FormContactMappingToLeadConverter"/>
    /// with custom mapping combined
    /// </summary>
    /// <param name="formCodeName"></param>
    /// <param name="configureFields"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DynamicsBizFormsMappingBuilder AddFormWithContactMapping(
        string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));

        AddFormWithConverter<FormContactMappingToLeadConverter>(formCodeName);
        return this;
    }

    /// <summary>
    /// Adds form with custom converter. Use this method when you want to have full control. You can add multiple
    /// converters for same form
    /// </summary>
    /// <param name="formCodeName"></param>
    /// <typeparam name="TConverter"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
            converters[formCodeName.ToLowerInvariant()] = new List<Type> { typeof(TConverter) };
        }

        serviceCollection.TryAddEnumerable(ServiceDescriptor
            .Scoped<ICRMTypeConverter<BizFormItem, Lead>, TConverter>());
        return this;
    }

    /// <summary>
    /// Adds custom service for BizForm item validation before sending to CRM
    /// </summary>
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