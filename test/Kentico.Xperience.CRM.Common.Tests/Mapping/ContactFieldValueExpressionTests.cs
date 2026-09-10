using Kentico.Xperience.CRM.Common.Mapping;

namespace Kentico.Xperience.CRM.Common.Tests.Mapping;

/// <summary>
/// Covers how a configured mapping survives a round trip through the database column it is stored in.
/// </summary>
[TestFixture]
public class ContactFieldValueExpressionTests
{
    [Test]
    public void RoundTrip_KeepsATemplate()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Template,
            Template = "{{ContactFirstName}} {{ContactLastName}}"
        };

        var restored = ContactFieldValueExpression.FromJson(expression.ToJson());

        Assert.Multiple(() =>
        {
            Assert.That(restored.Kind, Is.EqualTo(ContactFieldValueKind.Template));
            Assert.That(restored.Template, Is.EqualTo("{{ContactFirstName}} {{ContactLastName}}"));
        });
    }

    [Test]
    public void RoundTrip_KeepsTheOrderOfCoalescedFields()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Coalesce,
            SourceFields = new List<string> { "ContactBusinessPhone", "ContactMobilePhone" }
        };

        var restored = ContactFieldValueExpression.FromJson(expression.ToJson());

        Assert.That(restored.SourceFields,
            Is.EqualTo(new[] { "ContactBusinessPhone", "ContactMobilePhone" }));
    }

    [Test]
    public void RoundTrip_KeepsAResolver()
    {
        var expression = new ContactFieldValueExpression
        {
            Kind = ContactFieldValueKind.Resolver,
            Resolver = "country",
            SourceField = "ContactCountryID"
        };

        var restored = ContactFieldValueExpression.FromJson(expression.ToJson());

        Assert.Multiple(() =>
        {
            Assert.That(restored.Kind, Is.EqualTo(ContactFieldValueKind.Resolver));
            Assert.That(restored.Resolver, Is.EqualTo("country"));
            Assert.That(restored.SourceField, Is.EqualTo("ContactCountryID"));
        });
    }

    [Test]
    public void ToJson_WritesTheKindAsAName()
    {
        var expression = new ContactFieldValueExpression { Kind = ContactFieldValueKind.Constant };

        // Stored as a name rather than a number so that reordering the enum cannot silently change the
        // meaning of a stored mapping.
        Assert.That(expression.ToJson(), Does.Contain("\"Constant\""));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [TestCase("{ this is not json")]
    public void FromJson_FallsBackToAnEmptyExpression_WhenTheStoredValueCannotBeRead(string? json)
    {
        var restored = ContactFieldValueExpression.FromJson(json);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Kind, Is.EqualTo(ContactFieldValueKind.SourceField));
            Assert.That(restored.SourceField, Is.Null);
            Assert.That(restored.SourceFields, Is.Empty);
        });
    }
}