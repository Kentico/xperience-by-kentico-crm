using System.Linq.Expressions;

using CMS.OnlineForms;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Dynamics.Helpers;

using Microsoft.Xrm.Sdk;

namespace Kentico.Xperience.CRM.Dynamics.Configuration
{
    /// <summary>
    /// Specific extensions for mapping to Dynamics which enables mapping to generated Lead entity class
    /// </summary>
    public static class BizFormFieldsMappingBuilderExtensions
    {
        /// <summary>
        /// Map BizFormItem field name to CRM field defined in member expression.
        /// CRM field name is get always from from <see cref="AttributeLogicalNameAttribute"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="formFieldName"></param>
        /// <param name="expression"></param>
        /// <typeparam name="TLeadEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static BizFormFieldsMappingBuilder MapField<TLeadEntity>(
            this BizFormFieldsMappingBuilder builder, string formFieldName,
            Expression<Func<TLeadEntity, object>> expression)
            where TLeadEntity : Entity
        {
            string crmFieldName = EntityHelper.GetLogicalNameFromExpression(expression);
            if (crmFieldName == string.Empty)
            {
                throw new InvalidOperationException("Attribute name cannot be empty");
            }

            builder.MapField(formFieldName, crmFieldName);
            return builder;
        }

        /// <summary>
        /// Map BizFormItem value from function to CRM field defined in member expression.
        /// CRM field name is get always from <see cref="AttributeLogicalNameAttribute"/> on property model
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="mappingFunc"></param>
        /// <param name="expression"></param>
        /// <typeparam name="TBizFormItem"></typeparam>
        /// <typeparam name="TLeadEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static BizFormFieldsMappingBuilder MapField<TBizFormItem, TLeadEntity>(
            this BizFormFieldsMappingBuilder builder, Func<TBizFormItem, object> mappingFunc,
            Expression<Func<TLeadEntity, object>> expression)
            where TBizFormItem : BizFormItem
            where TLeadEntity : Entity
        {
            string crmFieldName = EntityHelper.GetLogicalNameFromExpression(expression);

            if (crmFieldName == string.Empty)
            {
                throw new InvalidOperationException("Attribute name cannot be empty");
            }

            return builder.MapField(mappingFunc, crmFieldName);
        }
    }
}