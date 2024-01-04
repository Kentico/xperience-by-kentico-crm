namespace Kentico.Xperience.CRM.Dynamics.Configuration;

/// <summary>
/// API config for Dataverse API
/// </summary>
public class DataverseApiConfig
{
    public string? DynamicsUrl { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ConnectionString { get; set; }

    public bool IsValid() => !string.IsNullOrWhiteSpace(ConnectionString) ||
                             (!string.IsNullOrWhiteSpace(DynamicsUrl) && !string.IsNullOrWhiteSpace(ClientId) &&
                              !string.IsNullOrWhiteSpace(ClientSecret));
}