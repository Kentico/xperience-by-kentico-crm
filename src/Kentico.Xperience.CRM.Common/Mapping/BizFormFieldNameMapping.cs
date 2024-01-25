using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// BizForm item field mapping based on form field name
/// </summary>
public class BizFormFieldNameMapping : IBizFormFieldMapping
{
    private readonly string formFieldName;

    public BizFormFieldNameMapping(string formFieldName)
    {
        this.formFieldName = formFieldName;
    }

    public object MapFormField(BizFormItem bizFormItem)
        => bizFormItem.GetValue(formFieldName);
}