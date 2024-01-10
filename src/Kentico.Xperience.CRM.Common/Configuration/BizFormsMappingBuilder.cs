using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common builder for BizForms to CRM leads configuration mapping 
/// </summary>
public class BizFormsMappingBuilder
{
    private readonly IServiceCollection serviceCollection;
    protected readonly Dictionary<string, BizFormFieldsMappingBuilder> forms = new();

    public BizFormsMappingBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public BizFormsMappingBuilder AddForm(string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));

        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));
        return this;
    }

    public BizFormsMappingBuilder AddForm(string formCodeName,
        BizFormFieldsMappingBuilder configuredBuilder)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));

        forms.Add(formCodeName.ToLowerInvariant(), configuredBuilder);
        return this;
    }
}