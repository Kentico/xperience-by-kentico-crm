using System.Text.Json.Serialization;

namespace Kentico.Xperience.CRM.SalesForce.Models;

public class QueryResultBase
{
    [JsonPropertyName("totalSize")] public int TotalSize { get; set; }

    [JsonPropertyName("done")] public bool Done { get; set; }
}