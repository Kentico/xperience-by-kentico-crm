using System.Linq.Expressions;
using System.Reflection;

namespace Kentico.Xperience.CRM.Common.Mapping.Implementations;

/// <summary>
/// CRM entity field mapping based on function
/// </summary>
/// <typeparam name="TCrmEntity"></typeparam>
public class CRMFieldMappingFunction<TCrmEntity> : ICRMFieldMapping
{
    private readonly Expression<Func<TCrmEntity, object?>> mappingFunc;

    public CRMFieldMappingFunction(Expression<Func<TCrmEntity, object?>> mappingFunc)
    {
        this.mappingFunc = mappingFunc;
    }

    public object? MapCrmField(TCrmEntity crmEntity, object value)
    {
        if (crmEntity is null)
            throw new ArgumentNullException(nameof(crmEntity));

        if (mappingFunc.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
            throw new InvalidOperationException("CRM field mapping failed, missing member expression");
                
        propertyInfo.SetValue(crmEntity, value);
        return propertyInfo.GetValue(crmEntity);
    }
}