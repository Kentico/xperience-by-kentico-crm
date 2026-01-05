using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Common mapping for BizForm item field
/// </summary>
public interface IBizFormFieldMapping
{
    object MapFormField(BizFormItem bizFormItem);
}