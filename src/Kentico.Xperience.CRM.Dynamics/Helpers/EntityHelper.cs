using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Xrm.Sdk;

namespace Kentico.Xperience.CRM.Dynamics.Helpers;

public static class EntityHelper
{
    /// <summary>
    /// Method name is returned from <see cref="AttributeLogicalNameAttribute"/>
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static string GetLogicalNameFromExpression<TValue, TEntity>(
        Expression<Func<TEntity, TValue>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return propertyInfo.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName ?? string.Empty;
        }

        return string.Empty;
    }
}