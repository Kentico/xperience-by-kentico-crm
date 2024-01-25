using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Converters;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.Salesforce.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Configuration;

/// <summary>
/// Mapping builder for BizForm to Salesforce mapping
/// </summary>
public class SalesforceBizFormsMappingBuilder
{
    private readonly IServiceCollection serviceCollection;
    protected readonly Dictionary<string, BizFormFieldsMappingBuilder> forms = new();
    protected readonly Dictionary<string, List<Type>> converters = new();

    public SalesforceBizFormsMappingBuilder(IServiceCollection serviceCollection)
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
    public SalesforceBizFormsMappingBuilder AddForm(string formCodeName,
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
    public SalesforceBizFormsMappingBuilder AddFormWithContactMapping(string formCodeName)
        => AddFormWithContactMapping(formCodeName, b => b);

    public SalesforceBizFormsMappingBuilder AddFormWithContactMapping(
        string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));

        AddFormWithConverter<FormContactMappingToLeadConverter>(formCodeName);
        return this;
    }

    /// <summary>
    /// Adds form when conversion is added automatically based on Form-Contact mapping <see cref="FormContactMappingToLeadConverter"/>
    /// with custom mapping combined
    /// </summary>
    /// <param name="formCodeName"></param>
    /// <typeparam name="TConverter"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public SalesforceBizFormsMappingBuilder AddFormWithConverter<TConverter>(string formCodeName)
        where TConverter : class, ICRMTypeConverter<BizFormItem, LeadSObject>
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
            .Scoped<ICRMTypeConverter<BizFormItem, LeadSObject>, TConverter>());
        return this;
    }

    /// <summary>
    /// Adds custom service for BizForm item validation before sending to CRM
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public SalesforceBizFormsMappingBuilder AddCustomValidation<TService>()
        where TService : class, ILeadsIntegrationValidationService
    {
        serviceCollection.AddSingleton<ILeadsIntegrationValidationService, TService>();

        return this;
    }

    internal SalesforceBizFormsMappingConfiguration Build()
    {
        return new SalesforceBizFormsMappingConfiguration
        {
            FormsMappings = forms.Select(f => (f.Key, f.Value.Build()))
                .ToDictionary(r => r.Key, r => r.Item2),
            FormsConverters = converters
        };
    }
}