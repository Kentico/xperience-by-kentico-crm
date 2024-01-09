using CMS.Core;
using CMS.OnlineForms.Internal;
using Kentico.Xperience.CRM.Common.Configuration;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

public static class BizFormMappingBuilderExtensions
{
    public static BizFormsMappingBuilder AddFormWithContactMapping(this BizFormsMappingBuilder formsMappingBuilder,
        string formCodeName) => AddFormWithContactMapping(formsMappingBuilder, formCodeName, b => b);

    public static BizFormsMappingBuilder AddFormWithContactMapping(this BizFormsMappingBuilder formsMappingBuilder,
        string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));
        //@TODO use Form contact mapping
        var mappingBuilder = new BizFormFieldsMappingBuilder();
        
        formsMappingBuilder.AddForm(formCodeName, mappingBuilder);
        return formsMappingBuilder;
    }

    private static void ConfigureMappingFromContactMapping(string formClassName, BizFormFieldsMappingBuilder mappingBuilder)
    {
    }
}