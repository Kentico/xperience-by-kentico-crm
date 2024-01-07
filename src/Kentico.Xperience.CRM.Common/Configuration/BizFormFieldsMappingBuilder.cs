using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common builder for mapping between BizForm field and Lead CRM field
/// </summary>
public class BizFormFieldsMappingBuilder
{
    protected List<BizFormFieldMapping> fieldMappings = new();

    public BizFormFieldsMappingBuilder MapField(string formFieldName, string crmFieldName)
    {
        fieldMappings.Add(new BizFormFieldMapping(new BizFormFieldNameMapping(formFieldName), new CRMFieldNameMapping(crmFieldName)));
        return this;
    }

    public BizFormFieldsMappingBuilder MapField<TBizFormItem>(Func<TBizFormItem, object> mappingFunc, string crmFieldName)
        where TBizFormItem : BizFormItem
    {
        fieldMappings.Add(new BizFormFieldMapping(new BizFormFieldMappingFunction<TBizFormItem>(mappingFunc), new CRMFieldNameMapping(crmFieldName)));
        return this;
    }

    public BizFormFieldsMappingBuilder AddMapping(BizFormFieldMapping mapping)
    {
        fieldMappings.Add(mapping);
        return this;
    }

    internal List<BizFormFieldMapping> Build() => fieldMappings;
}