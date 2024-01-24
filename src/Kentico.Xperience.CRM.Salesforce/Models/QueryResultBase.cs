using System.Text.Json.Serialization;

namespace Kentico.Xperience.CRM.Salesforce.Models;

/// <summary>
/// Base model for query result
/// </summary>
public class QueryResultBase
{
    [JsonPropertyName("totalSize")] public int TotalSize { get; set; }

    [JsonPropertyName("done")] public bool Done { get; set; }
}