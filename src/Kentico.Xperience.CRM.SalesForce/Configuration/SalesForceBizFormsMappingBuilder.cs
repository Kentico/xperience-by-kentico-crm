using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Services;
using Kentico.Xperience.CRM.SalesForce.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SalesForce.OpenApi;

namespace Kentico.Xperience.CRM.SalesForce.Configuration;

public class SalesForceBizFormsMappingBuilder
{
    private readonly IServiceCollection serviceCollection;
    protected readonly Dictionary<string, BizFormFieldsMappingBuilder> forms = new();
    protected readonly Dictionary<string, List<Type>> converters = new();
    
    public SalesForceBizFormsMappingBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }
    
    public SalesForceBizFormsMappingBuilder AddForm(string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));

        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));
        return this;
    }

    public SalesForceBizFormsMappingBuilder AddForm(string formCodeName,
        BizFormFieldsMappingBuilder configuredBuilder)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));

        forms.Add(formCodeName.ToLowerInvariant(), configuredBuilder);
        return this;
    }
    
    public SalesForceBizFormsMappingBuilder AddFormWithContactMapping(string formCodeName) 
        => AddFormWithContactMapping(formCodeName, b => b);

    public SalesForceBizFormsMappingBuilder AddFormWithContactMapping(
        string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));

        AddFormWithConverter<FormContactMappingToLeadConverter>(formCodeName);
        return this;
    }

    public SalesForceBizFormsMappingBuilder AddFormWithConverter<TConverter>(string formCodeName)
        where TConverter : class, ICRMTypeConverter<BizFormItem, LeadSObject>
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
        
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<ICRMTypeConverter<BizFormItem, LeadSObject>, TConverter>());
        return this;
    }
    
    /// <summary>
    /// Adds custom service for BizForm item validation before sending to CRM
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public SalesForceBizFormsMappingBuilder AddCustomValidation<TService>()
        where TService : class, ILeadsIntegrationValidationService
    {
        serviceCollection.AddSingleton<ILeadsIntegrationValidationService, TService>();

        return this;
    }
    
    internal SalesForceBizFormsMappingConfiguration Build()
    {
        return new SalesForceBizFormsMappingConfiguration
        {
            FormsMappings = forms.Select(f => (f.Key, f.Value.Build()))
                .ToDictionary(r => r.Key, r => r.Item2),
            FormsConverters = converters
        };
    }

}