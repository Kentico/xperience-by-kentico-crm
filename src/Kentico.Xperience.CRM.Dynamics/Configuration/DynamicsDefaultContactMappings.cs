using System.Linq.Expressions;

using CMS.ContactManagement;

using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Constants;
using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;
using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;
using Kentico.Xperience.CRM.Dynamics.Helpers;

using Microsoft.Xrm.Sdk;

namespace Kentico.Xperience.CRM.Dynamics.Configuration;

/// <summary>
/// The mapping the Dynamics integration applies out of the box, expressed as configuration so the admin UI
/// can offer it as a starting point.
/// Mirrors <see cref="DynamicsContactMappingBuilder.AddDefaultMappingForLead"/> and
/// <see cref="DynamicsContactMappingBuilder.AddDefaultMappingForContact"/> - keep the three in step.
/// CRM field names are read from the compiled entity classes rather than written out, so a renamed
/// attribute in the generated entities cannot leave this list pointing at a field that does not exist.
/// </summary>
internal static class DynamicsDefaultContactMappings
{
    public static IReadOnlyList<ContactFieldMappingDefinition> Get(string entityType) =>
        string.Equals(entityType, EntityType.Contact, StringComparison.OrdinalIgnoreCase)
            ? GetForContact()
            : GetForLead();

    private static List<ContactFieldMappingDefinition> GetForLead() => new()
    {
        FromField<Lead>(l => l.FirstName, nameof(ContactInfo.ContactFirstName)),
        FromField<Lead>(l => l.MiddleName, nameof(ContactInfo.ContactMiddleName)),
        FromField<Lead>(l => l.LastName, nameof(ContactInfo.ContactLastName)),
        FromField<Lead>(l => l.EMailAddress1, nameof(ContactInfo.ContactEmail)),
        FromField<Lead>(l => l.Address1_Line1, nameof(ContactInfo.ContactAddress1)),
        FromField<Lead>(l => l.Address1_City, nameof(ContactInfo.ContactCity)),
        FromField<Lead>(l => l.Address1_PostalCode, nameof(ContactInfo.ContactZIP)),
        FromResolver<Lead>(l => l.Address1_Country, CountryNameResolver.ResolverName,
            nameof(ContactInfo.ContactCountryID)),
        FromField<Lead>(l => l.JobTitle, nameof(ContactInfo.ContactJobTitle)),
        FromField<Lead>(l => l.MobilePhone, nameof(ContactInfo.ContactMobilePhone)),
        FromField<Lead>(l => l.Telephone1, nameof(ContactInfo.ContactBusinessPhone)),
        FromField<Lead>(l => l.CompanyName, nameof(ContactInfo.ContactCompanyName)),
        FromField<Lead>(l => l.Description, nameof(ContactInfo.ContactNotes))
    };

    private static List<ContactFieldMappingDefinition> GetForContact() => new()
    {
        FromField<Contact>(c => c.FirstName, nameof(ContactInfo.ContactFirstName)),
        FromField<Contact>(c => c.MiddleName, nameof(ContactInfo.ContactMiddleName)),
        FromField<Contact>(c => c.LastName, nameof(ContactInfo.ContactLastName)),
        FromField<Contact>(c => c.EMailAddress1, nameof(ContactInfo.ContactEmail)),
        FromField<Contact>(c => c.Address1_Line1, nameof(ContactInfo.ContactAddress1)),
        FromField<Contact>(c => c.Address1_City, nameof(ContactInfo.ContactCity)),
        FromField<Contact>(c => c.Address1_PostalCode, nameof(ContactInfo.ContactZIP)),
        FromResolver<Contact>(c => c.Address1_Country, CountryNameResolver.ResolverName,
            nameof(ContactInfo.ContactCountryID)),
        FromField<Contact>(c => c.JobTitle, nameof(ContactInfo.ContactJobTitle)),
        FromField<Contact>(c => c.MobilePhone, nameof(ContactInfo.ContactMobilePhone)),
        FromField<Contact>(c => c.Telephone1, nameof(ContactInfo.ContactBusinessPhone)),
        FromField<Contact>(c => c.Description, nameof(ContactInfo.ContactNotes))
    };

    private static ContactFieldMappingDefinition FromField<TEntity>(
        Expression<Func<TEntity, object>> crmField, string contactField)
        where TEntity : Entity => new()
        {
            CRMField = GetCrmFieldName(crmField),
            Expression = new ContactFieldValueExpression
            {
                Kind = ContactFieldValueKind.SourceField,
                SourceField = contactField
            }
        };

    private static ContactFieldMappingDefinition FromResolver<TEntity>(
        Expression<Func<TEntity, object>> crmField, string resolver, string contactField)
        where TEntity : Entity => new()
        {
            CRMField = GetCrmFieldName(crmField),
            Expression = new ContactFieldValueExpression
            {
                Kind = ContactFieldValueKind.Resolver,
                Resolver = resolver,
                SourceField = contactField
            }
        };

    private static string GetCrmFieldName<TEntity>(Expression<Func<TEntity, object>> crmField)
    {
        string name = EntityHelper.GetLogicalNameFromExpression(crmField);

        if (name.Length == 0)
        {
            throw new InvalidOperationException(
                "Default mapping refers to a property without an attribute logical name");
        }

        return name;
    }
}