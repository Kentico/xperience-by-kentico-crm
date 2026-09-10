using System.Globalization;
using System.Text.RegularExpressions;

namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Evaluates a <see cref="ContactFieldValueExpression"/> into the value written to a CRM field.
/// Deliberately free of any dependency on the contact object itself - the caller supplies the field values
/// and the resolvers - which keeps the combining rules in one testable place.
/// </summary>
public static class ContactFieldValueEvaluator
{
    /// <summary>
    /// Matches a <c>{{ContactField}}</c> token. Whitespace inside the braces is tolerated.
    /// </summary>
    private static readonly Regex TokenRegex = new(@"\{\{\s*([A-Za-z0-9_]+)\s*\}\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Produces the value of the expression. Never returns <see langword="null"/> - an expression which
    /// resolves to nothing produces an empty string, which is what the CRM specific code treats as
    /// "no value" and skips.
    /// </summary>
    /// <param name="expression">Expression to evaluate.</param>
    /// <param name="getFieldValue">Returns the value of a contact field by name.</param>
    /// <param name="resolve">Returns the value of a named resolver for an optional contact field.</param>
    public static object Evaluate(ContactFieldValueExpression expression,
        Func<string, object?> getFieldValue,
        Func<string, string?, object?> resolve)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        return expression.Kind switch
        {
            ContactFieldValueKind.SourceField => GetFieldValue(getFieldValue, expression.SourceField),
            ContactFieldValueKind.Template => ResolveTemplate(expression, getFieldValue),
            ContactFieldValueKind.Constant => expression.ConstantValue ?? string.Empty,
            ContactFieldValueKind.Coalesce => ResolveCoalesce(expression, getFieldValue),
            ContactFieldValueKind.Resolver => ResolveWithResolver(expression, resolve),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Returns the contact field names the expression reads, so that a mapping referring to a field which
    /// no longer exists can be reported.
    /// </summary>
    public static IEnumerable<string> GetReferencedFields(ContactFieldValueExpression expression)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        return EnumerateReferencedFields(expression);
    }

    private static IEnumerable<string> EnumerateReferencedFields(ContactFieldValueExpression expression)
    {
        switch (expression.Kind)
        {
            case ContactFieldValueKind.SourceField:
            case ContactFieldValueKind.Resolver:
                if (!string.IsNullOrWhiteSpace(expression.SourceField))
                {
                    yield return expression.SourceField;
                }

                break;

            case ContactFieldValueKind.Template:
                foreach (var match in TokenRegex.Matches(expression.Template ?? string.Empty).Cast<Match>())
                {
                    yield return match.Groups[1].Value;
                }

                break;

            case ContactFieldValueKind.Coalesce:
                foreach (string field in expression.SourceFields.Where(f => !string.IsNullOrWhiteSpace(f)))
                {
                    yield return field;
                }

                break;

            case ContactFieldValueKind.Constant:
            default:
                break;
        }
    }

    private static object GetFieldValue(Func<string, object?> getFieldValue, string? fieldName) =>
        string.IsNullOrWhiteSpace(fieldName) ? string.Empty : getFieldValue(fieldName) ?? string.Empty;

    /// <summary>
    /// Replaces every token with the contact field value. An unknown token becomes an empty string, so a
    /// renamed contact field degrades the produced text instead of failing the synchronization.
    /// </summary>
    private static object ResolveTemplate(ContactFieldValueExpression expression,
        Func<string, object?> getFieldValue)
    {
        if (string.IsNullOrEmpty(expression.Template))
        {
            return string.Empty;
        }

        string result = TokenRegex.Replace(expression.Template,
            match => Convert.ToString(getFieldValue(match.Groups[1].Value), CultureInfo.InvariantCulture)
                     ?? string.Empty);

        // A template whose tokens all resolved to nothing leaves only the literals and whitespace, which
        // is not a meaningful CRM value.
        return string.IsNullOrWhiteSpace(result) ? string.Empty : result.Trim();
    }

    private static object ResolveCoalesce(ContactFieldValueExpression expression,
        Func<string, object?> getFieldValue)
    {
        foreach (string fieldName in expression.SourceFields.Where(f => !string.IsNullOrWhiteSpace(f)))
        {
            object value = GetFieldValue(getFieldValue, fieldName);

            if (value is string text)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
            else if (!IsEmptyValue(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static object ResolveWithResolver(ContactFieldValueExpression expression,
        Func<string, string?, object?> resolve)
    {
        if (string.IsNullOrWhiteSpace(expression.Resolver))
        {
            return string.Empty;
        }

        return resolve(expression.Resolver, expression.SourceField) ?? string.Empty;
    }

    /// <summary>
    /// Treats the default value of the common Xperience field types as "no value", so that coalescing
    /// skips an unset reference field rather than sending a zero.
    /// </summary>
    private static bool IsEmptyValue(object? value) => value switch
    {
        null => true,
        int i => i == 0,
        Guid g => g == Guid.Empty,
        DateTime d => d == DateTime.MinValue,
        _ => false
    };
}