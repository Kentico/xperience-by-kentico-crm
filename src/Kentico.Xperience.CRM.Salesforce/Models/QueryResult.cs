using System.Text.Json.Serialization;

namespace Kentico.Xperience.CRM.Salesforce.Models;

/// <summary>
/// Query result
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryResult<T> : QueryResultBase
{
    [JsonPropertyName("records")] public List<T> Records { get; set; } = new();
}