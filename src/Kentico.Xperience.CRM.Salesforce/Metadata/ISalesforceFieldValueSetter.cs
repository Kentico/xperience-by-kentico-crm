namespace Kentico.Xperience.CRM.Salesforce.Metadata;

/// <summary>
/// Writes a mapped value onto an sObject addressed by Salesforce field name.
/// Mappings configured in the admin UI only know the field name, so the value has to be routed either to
/// the generated property of the sObject or, for a field the generated class does not have, to the
/// extension data which is serialized alongside it.
/// </summary>
internal interface ISalesforceFieldValueSetter
{
    /// <summary>
    /// Sets the value of a single field.
    /// </summary>
    /// <param name="sObject">sObject instance being filled.</param>
    /// <param name="additionalProperties">Extension data of <paramref name="sObject"/>, used for fields
    /// the generated class does not declare, such as custom fields.</param>
    /// <param name="entityType">Entity type. See <see cref="Common.Constants.EntityType"/>.</param>
    /// <param name="fieldName">Salesforce field name.</param>
    /// <param name="value">Value produced by the mapping.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetFieldAsync(object sObject, IDictionary<string, object> additionalProperties, string entityType,
        string fieldName, object? value, CancellationToken cancellationToken = default);
}