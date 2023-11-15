using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Mapping.Implementations;

/// <summary>
/// BizForm item field mapping based on function
/// </summary>
/// <typeparam name="TBizFormFieldItem"></typeparam>
public class BizFormFieldMappingFunction<TBizFormFieldItem> : IBizFormFieldMapping
    where TBizFormFieldItem : BizFormItem
{
    private readonly Func<TBizFormFieldItem, object> mappingFunc;

    public BizFormFieldMappingFunction(Func<TBizFormFieldItem, object> mappingFunc)
    {
        this.mappingFunc = mappingFunc;
    }
    
    public object MapFormField(BizFormItem bizFormItem)
    {
        if (bizFormItem is not TBizFormFieldItem item)
            throw new InvalidOperationException("Cannot map");

        return mappingFunc(item);
    }
}
