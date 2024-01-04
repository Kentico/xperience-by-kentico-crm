using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using SalesForce.OpenApi;
using System.Linq.Expressions;

namespace Kentico.Xperience.CRM.SalesForce.Configuration;

/// <summary>
/// Specific extensions for mapping to SalesForce which enables mapping to generated Lead entity class
/// </summary>
public static class BizFormFieldsMappingBuilderExtensions
{
    /// <summary>
    /// Map BizFormItem field name to CRM field defined in member expression.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="formFieldName"></param>
    /// <param name="expression"></param>    
    /// <returns></returns>
    public static BizFormFieldsMappingBuilder MapField(
        this BizFormFieldsMappingBuilder builder, string formFieldName,
        Expression<Func<LeadSObject, object>> expression)
    {
        return builder.AddMapping(new BizFormFieldMapping(new BizFormFieldNameMapping(formFieldName),
            new CRMFieldMappingFunction<LeadSObject>(expression)));
    }

    /// <summary>
    /// Map BizFormItem value from function to CRM field defined in member expression.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="formMappingFunc"></param>
    /// <param name="crmMappingFunc"></param>
    /// <typeparam name="TBizFormItem"></typeparam>    
    /// <returns></returns>
    public static BizFormFieldsMappingBuilder MapField<TBizFormItem>(
        this BizFormFieldsMappingBuilder builder, Func<TBizFormItem, object> formMappingFunc,
        Expression<Func<LeadSObject, object>> crmMappingFunc)
        where TBizFormItem : BizFormItem
    {
        return builder.AddMapping(new BizFormFieldMapping(
            new BizFormFieldMappingFunction<TBizFormItem>(formMappingFunc),
            new CRMFieldMappingFunction<LeadSObject>(crmMappingFunc)));
    }
}