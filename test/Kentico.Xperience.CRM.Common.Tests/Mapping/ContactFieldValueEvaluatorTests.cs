using Kentico.Xperience.CRM.Common.Mapping;

namespace Kentico.Xperience.CRM.Common.Tests.Mapping;

/// <summary>
/// Covers how a configured mapping turns contact data into the value sent to the CRM.
/// </summary>
[TestFixture]
public class ContactFieldValueEvaluatorTests
{
    private static readonly Dictionary<string, object?> ContactFields = new()
    {
        ["ContactFirstName"] = "Jane",
        ["ContactLastName"] = "Doe",
        ["ContactEmail"] = "jane.doe@example.com",
        ["ContactMiddleName"] = string.Empty,
        ["ContactBusinessPhone"] = "   ",
        ["ContactMobilePhone"] = "+1 555 0100",
        ["ContactCountryID"] = 42,
        ["ContactStateID"] = 0,
        ["ContactNotes"] = null
    };

    private static object Evaluate(ContactFieldValueExpression expression,
        Func<string, string?, object?>? resolve = null) =>
        ContactFieldValueEvaluator.Evaluate(expression,
            fieldName => ContactFields.GetValueOrDefault(fieldName),
            resolve ?? ((_, _) => null));

    [Test]
    public void SourceField_ReturnsTheFieldValue()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.SourceField,
            SourceField = "ContactFirstName"
        };

        Assert.That(Evaluate(expression), Is.EqualTo("Jane"));
    }

    [Test]
    public void SourceField_ReturnsEmptyValue_WhenFieldDoesNotExist()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.SourceField,
            SourceField = "ContactFieldRemovedFromTheProject"
        };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void SourceField_ReturnsEmptyValue_WhenNoFieldIsSelected()
    {
        var expression = new ContactFieldValueExpression { Kind = ContactFieldValueKind.SourceField };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Template_CombinesSeveralFieldsIntoOneValue()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "{{ContactFirstName}} {{ContactLastName}}"
        };

        Assert.That(Evaluate(expression), Is.EqualTo("Jane Doe"));
    }

    [Test]
    public void Template_KeepsLiteralText()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "Lead from {{ContactEmail}} (web)"
        };

        Assert.That(Evaluate(expression), Is.EqualTo("Lead from jane.doe@example.com (web)"));
    }

    [Test]
    public void Template_ToleratesWhitespaceInsideTokens()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "{{ ContactFirstName }}"
        };

        Assert.That(Evaluate(expression), Is.EqualTo("Jane"));
    }

    [Test]
    public void Template_ReplacesUnknownTokenWithNothing()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "{{ContactFirstName}} {{ContactFieldRemovedFromTheProject}}"
        };

        Assert.That(Evaluate(expression), Is.EqualTo("Jane"));
    }

    [Test]
    public void Template_ReturnsEmptyValue_WhenEveryTokenResolvesToNothing()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "{{ContactMiddleName}} {{ContactNotes}}"
        };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Template_ReturnsEmptyValue_WhenTemplateIsEmpty()
    {
        var expression = new ContactFieldValueExpression { Kind = ContactFieldValueKind.Template };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constant_ReturnsTheLiteral()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Constant,
            ConstantValue = "Xperience web"
        };

        Assert.That(Evaluate(expression), Is.EqualTo("Xperience web"));
    }

    [Test]
    public void Coalesce_ReturnsTheFirstFieldWithAValue()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Coalesce,
            SourceFields = new List<string> { "ContactBusinessPhone", "ContactMobilePhone" }
        };

        Assert.That(Evaluate(expression), Is.EqualTo("+1 555 0100"));
    }

    [Test]
    public void Coalesce_SkipsUnsetReferenceFields()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Coalesce,
            SourceFields = new List<string> { "ContactStateID", "ContactCountryID" }
        };

        Assert.That(Evaluate(expression), Is.EqualTo(42));
    }

    [Test]
    public void Coalesce_ReturnsEmptyValue_WhenNoFieldHasAValue()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Coalesce,
            SourceFields = new List<string> { "ContactMiddleName", "ContactNotes" }
        };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Coalesce_ReturnsEmptyValue_WhenNoFieldIsListed()
    {
        var expression = new ContactFieldValueExpression { Kind = ContactFieldValueKind.Coalesce };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Resolver_ReturnsTheResolvedValue()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Resolver,
            Resolver = "country",
            SourceField = "ContactCountryID"
        };

        object value = Evaluate(expression,
            (resolverName, sourceField) =>
                resolverName == "country" && sourceField == "ContactCountryID" ? "Canada" : null);

        Assert.That(value, Is.EqualTo("Canada"));
    }

    [Test]
    public void Resolver_ReturnsEmptyValue_WhenTheResolverIsNotRegistered()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Resolver,
            Resolver = "resolver-removed-from-the-project",
            SourceField = "ContactCountryID"
        };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Resolver_ReturnsEmptyValue_WhenNoResolverIsSelected()
    {
        var expression = new ContactFieldValueExpression { Kind = ContactFieldValueKind.Resolver };

        Assert.That(Evaluate(expression), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Evaluate_Throws_WhenExpressionIsMissing()
    {
        Assert.That(
            () => ContactFieldValueEvaluator.Evaluate(null!, _ => null, (_, _) => null),
            Throws.ArgumentNullException);
    }

    [Test]
    public void GetReferencedFields_ReturnsTheSelectedField()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.SourceField,
            SourceField = "ContactFirstName"
        };

        Assert.That(ContactFieldValueEvaluator.GetReferencedFields(expression),
            Is.EqualTo(new[] { "ContactFirstName" }));
    }

    [Test]
    public void GetReferencedFields_ReturnsEveryTokenOfATemplate()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "{{ContactFirstName}} {{ContactLastName}} <{{ContactEmail}}>"
        };

        Assert.That(ContactFieldValueEvaluator.GetReferencedFields(expression),
            Is.EqualTo(new[] { "ContactFirstName", "ContactLastName", "ContactEmail" }));
    }

    [Test]
    public void GetReferencedFields_ReturnsEveryCoalescedField()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Coalesce,
            SourceFields = new List<string> { "ContactBusinessPhone", "ContactMobilePhone" }
        };

        Assert.That(ContactFieldValueEvaluator.GetReferencedFields(expression),
            Is.EqualTo(new[] { "ContactBusinessPhone", "ContactMobilePhone" }));
    }

    [Test]
    public void GetReferencedFields_ReturnsNothingForAConstant()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Constant,
            ConstantValue = "Xperience web"
        };

        Assert.That(ContactFieldValueEvaluator.GetReferencedFields(expression), Is.Empty);
    }
}