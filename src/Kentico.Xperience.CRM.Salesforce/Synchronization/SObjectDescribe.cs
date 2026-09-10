using System.Text.Json.Serialization;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

/// <summary>
/// Subset of the Salesforce sObject describe response needed to offer the fields of an sObject
/// in the field mapping UI.
/// </summary>
internal class SObjectDescribe
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("fields")]
    public List<SObjectFieldDescribe> Fields { get; set; } = new();
}

/// <summary>
/// Describes a single field of an sObject.
/// </summary>
internal class SObjectFieldDescribe
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Salesforce field type, for example <c>string</c>, <c>picklist</c> or <c>currency</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("length")]
    public int? Length { get; set; }

    /// <summary>
    /// <see langword="false"/> when the field cannot be left empty.
    /// </summary>
    [JsonPropertyName("nillable")]
    public bool Nillable { get; set; }

    [JsonPropertyName("createable")]
    public bool Createable { get; set; }

    [JsonPropertyName("updateable")]
    public bool Updateable { get; set; }

    /// <summary>
    /// <see langword="true"/> when Salesforce fills the field itself, which means a value is not required
    /// even though the field is not nillable.
    /// </summary>
    [JsonPropertyName("defaultedOnCreate")]
    public bool DefaultedOnCreate { get; set; }

    [JsonPropertyName("custom")]
    public bool Custom { get; set; }

    [JsonPropertyName("picklistValues")]
    public List<SObjectPicklistValue> PicklistValues { get; set; } = new();

    [JsonPropertyName("referenceTo")]
    public List<string> ReferenceTo { get; set; } = new();
}

/// <summary>
/// Single allowed value of a picklist field.
/// </summary>
internal class SObjectPicklistValue
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}