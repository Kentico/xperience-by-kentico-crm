using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

using CMS.ContactManagement;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;

using Salesforce.OpenApi;

namespace Kentico.Xperience.CRM.Salesforce.Configuration;

/// <summary>
/// The mapping the Salesforce integration applies out of the box, expressed as configuration so the admin
/// UI can offer it as a starting point.
/// Mirrors <see cref="SalesforceContactMappingBuilder.AddDefaultMappingForLead"/> and
/// <see cref="SalesforceContactMappingBuilder.AddDefaultMappingForContact"/> - keep the three in step.
/// Salesforce field names are read from the generated sObject classes rather than written out, so a
/// regenerated client cannot leave this list pointing at a field that does not exist.
/// </summary>
internal static class SalesforceDefaultContactMappings
{
    public static IReadOnlyList<ContactFieldMappingDefinition> Get(string entityType) =>
        string.Equals(entityType, EntityType.Contact, StringComparison.OrdinalIgnoreCase)
            ? GetForContact()
            : GetForLead();

    private static List<ContactFieldMappingDefinition> GetForLead() => new()
    {
        FromField<LeadSObject>(l => l.FirstName, nameof(ContactInfo.ContactFirstName)),
        FromField<LeadSObject>(l => l.LastName, nameof(ContactInfo.ContactLastName)),
        FromField<LeadSObject>(l => l.Email, nameof(ContactInfo.ContactEmail)),
        FromField<LeadSObject>(l => l.Street, nameof(ContactInfo.ContactAddress1)),
        FromField<LeadSObject>(l => l.City, nameof(ContactInfo.ContactCity)),
        FromField<LeadSObject>(l => l.PostalCode, nameof(ContactInfo.ContactZIP)),
        FromResolver<LeadSObject>(l => l.Country, CountryNameResolver.ResolverName,
            nameof(ContactInfo.ContactCountryID)),
        FromField<LeadSObject>(l => l.MobilePhone, nameof(ContactInfo.ContactMobilePhone)),
        FromField<LeadSObject>(l => l.Phone, nameof(ContactInfo.ContactBusinessPhone)),
        FromField<LeadSObject>(l => l.Company, nameof(ContactInfo.ContactCompanyName)),
        FromField<LeadSObject>(l => l.Description, nameof(ContactInfo.ContactNotes))
    };

    private static List<ContactFieldMappingDefinition> GetForContact() => new()
    {
        FromField<ContactSObject>(c => c.FirstName, nameof(ContactInfo.ContactFirstName)),
        FromField<ContactSObject>(c => c.LastName, nameof(ContactInfo.ContactLastName)),
        FromField<ContactSObject>(c => c.Email, nameof(ContactInfo.ContactEmail)),
        FromField<ContactSObject>(c => c.MailingStreet, nameof(ContactInfo.ContactAddress1)),
        FromField<ContactSObject>(c => c.MailingCity, nameof(ContactInfo.ContactCity)),
        FromField<ContactSObject>(c => c.MailingPostalCode, nameof(ContactInfo.ContactZIP)),
        FromResolver<ContactSObject>(c => c.MailingCountry, CountryNameResolver.ResolverName,
            nameof(ContactInfo.ContactCountryID)),
        FromField<ContactSObject>(c => c.MobilePhone, nameof(ContactInfo.ContactMobilePhone)),
        FromField<ContactSObject>(c => c.Phone, nameof(ContactInfo.ContactBusinessPhone)),
        FromField<ContactSObject>(c => c.Description, nameof(ContactInfo.ContactNotes))
    };

    private static ContactFieldMappingDefinition FromField<TSObject>(
        Expression<Func<TSObject, object?>> crmField, string contactField) => new()
        {
            CRMField = GetCrmFieldName(crmField),
            Expression = new ContactFieldValueExpression
            {
                Kind = ContactFieldValueKind.SourceField,
                SourceField = contactField
            }
        };

    private static ContactFieldMappingDefinition FromResolver<TSObject>(
        Expression<Func<TSObject, object?>> crmField, string resolver, string contactField) => new()
        {
            CRMField = GetCrmFieldName(crmField),
            Expression = new ContactFieldValueExpression
            {
                Kind = ContactFieldValueKind.Resolver,
                Resolver = resolver,
                SourceField = contactField
            }
        };

    /// <summary>
    /// Reads the Salesforce field name from the serialization attribute of the referenced property, which
    /// is the same name the REST API uses.
    /// </summary>
    private static string GetCrmFieldName<TSObject>(Expression<Func<TSObject, object?>> crmField)
    {
        // A conversion node appears when the referenced property is a value type.
        var body = crmField.Body is UnaryExpression { NodeType: ExpressionType.Convert } unary
            ? unary.Operand
            : crmField.Body;

        if (body is not MemberExpression { Member: PropertyInfo property })
        {
            throw new InvalidOperationException("Default mapping must refer to an sObject property");
        }

        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }
}