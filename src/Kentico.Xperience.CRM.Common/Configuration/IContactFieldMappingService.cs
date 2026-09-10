using Kentico.Xperience.CRM.Common.Mapping;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Reads and writes the visually configured contact field mappings, and compiles them into the mappings
/// consumed by the synchronization services.
/// </summary>
public interface IContactFieldMappingService
{
    /// <summary>
    /// Returns the stored mappings for a CRM and entity type, ordered as configured.
    /// </summary>
    /// <param name="crmName">CRM name. See <see cref="Constants.CRMType"/>.</param>
    /// <param name="entityType">Entity type. See <see cref="Constants.EntityType"/>.</param>
    IReadOnlyList<ContactFieldMappingDefinition> GetDefinitions(string crmName, string entityType);

    /// <summary>
    /// Replaces the stored mappings for a CRM and entity type with the given set. Passing an empty set
    /// removes the configuration, which returns the integration to the mappings defined in code.
    /// </summary>
    /// <param name="crmName">CRM name. See <see cref="Constants.CRMType"/>.</param>
    /// <param name="entityType">Entity type. See <see cref="Constants.EntityType"/>.</param>
    /// <param name="definitions">Mappings to store.</param>
    void SaveDefinitions(string crmName, string entityType,
        IEnumerable<ContactFieldMappingDefinition> definitions);

    /// <summary>
    /// Indicates whether any mapping is stored for a CRM and entity type.
    /// </summary>
    bool HasConfiguration(string crmName, string entityType);

    /// <summary>
    /// Returns the mappings the synchronization should apply. When a configuration exists it fully replaces
    /// <paramref name="codeMappings"/>; otherwise <paramref name="codeMappings"/> is returned unchanged.
    /// </summary>
    /// <param name="crmName">CRM name. See <see cref="Constants.CRMType"/>.</param>
    /// <param name="entityType">Entity type. See <see cref="Constants.EntityType"/>.</param>
    /// <param name="codeMappings">Mappings registered on startup through the mapping builders.</param>
    IReadOnlyList<ContactFieldToCRMMapping> GetEffectiveMappings(string crmName, string entityType,
        IEnumerable<ContactFieldToCRMMapping> codeMappings);

    /// <summary>
    /// Evaluates a single expression against a contact. Used by the admin UI to preview a configured value.
    /// </summary>
    object Evaluate(ContactFieldValueExpression expression, CMS.ContactManagement.ContactInfo contactInfo);
}