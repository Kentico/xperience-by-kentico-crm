using CMS.ContactManagement;

using Kentico.Xperience.CRM.Common.Mapping.Resolvers;

namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Contact side of a mapping which is described by a <see cref="ContactFieldValueExpression"/> instead of
/// compiled code. This is what the visually configured mappings are compiled into, which lets configured
/// mappings flow through the same synchronization pipeline as the code based ones.
/// </summary>
public class ExpressionContactFieldMapping : IContactFieldMapping
{
    private readonly ContactFieldValueExpression expression;
    private readonly IReadOnlyDictionary<string, IContactSourceValueResolver> resolvers;

    public ExpressionContactFieldMapping(ContactFieldValueExpression expression,
        IReadOnlyDictionary<string, IContactSourceValueResolver> resolvers)
    {
        this.expression = expression;
        this.resolvers = resolvers;
    }

    /// <summary>
    /// Returns the contact field names the expression reads.
    /// </summary>
    public IEnumerable<string> GetReferencedFields() =>
        ContactFieldValueEvaluator.GetReferencedFields(expression);

    public object MapContactField(ContactInfo contactInfo)
    {
        if (contactInfo is null)
        {
            throw new ArgumentNullException(nameof(contactInfo));
        }

        return ContactFieldValueEvaluator.Evaluate(expression,
            fieldName => contactInfo.GetValue(fieldName),
            (resolverName, sourceField) => resolvers.TryGetValue(resolverName, out var resolver)
                ? resolver.Resolve(contactInfo, sourceField)
                : null);
    }
}