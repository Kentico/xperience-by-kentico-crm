using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Serializable description of how the value for a single CRM field is produced from a contact.
/// Persisted as JSON in <c>CRMContactFieldMappingExpression</c> and edited through the admin UI.
/// </summary>
public class ContactFieldValueExpression
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Determines which of the remaining members is used.
    /// </summary>
    public ContactFieldValueKind Kind { get; set; } = ContactFieldValueKind.SourceField;

    /// <summary>
    /// Contact field name. Used by <see cref="ContactFieldValueKind.SourceField"/> and
    /// <see cref="ContactFieldValueKind.Resolver"/>.
    /// </summary>
    public string? SourceField { get; set; }

    /// <summary>
    /// Template containing literals and <c>{{ContactField}}</c> tokens.
    /// Used by <see cref="ContactFieldValueKind.Template"/>.
    /// </summary>
    public string? Template { get; set; }

    /// <summary>
    /// Fixed literal. Used by <see cref="ContactFieldValueKind.Constant"/>.
    /// </summary>
    public string? ConstantValue { get; set; }

    /// <summary>
    /// Ordered contact field names. Used by <see cref="ContactFieldValueKind.Coalesce"/>.
    /// </summary>
    public List<string> SourceFields { get; set; } = new();

    /// <summary>
    /// Name of a registered <see cref="Resolvers.IContactSourceValueResolver"/>.
    /// Used by <see cref="ContactFieldValueKind.Resolver"/>.
    /// </summary>
    public string? Resolver { get; set; }

    /// <summary>
    /// Serializes the expression for storage.
    /// </summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);

    /// <summary>
    /// Deserializes a stored expression. Returns an empty <see cref="ContactFieldValueKind.SourceField"/>
    /// expression when the stored value cannot be parsed, so that a single malformed row cannot break a sync.
    /// </summary>
    public static ContactFieldValueExpression FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ContactFieldValueExpression();
        }

        try
        {
            return JsonSerializer.Deserialize<ContactFieldValueExpression>(json, SerializerOptions)
                   ?? new ContactFieldValueExpression();
        }
        catch (JsonException)
        {
            return new ContactFieldValueExpression();
        }
    }
}