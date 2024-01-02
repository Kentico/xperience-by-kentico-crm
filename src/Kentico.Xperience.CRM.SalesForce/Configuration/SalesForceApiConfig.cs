namespace Kentico.Xperience.CRM.SalesForce.Configuration;

/// <summary>
/// API config for SalesForce API
/// </summary>
public class SalesForceApiConfig
{
    public const decimal DefaultVersion = 59m;
    
    public string? SalesForceUrl { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    private decimal apiVersion;
    public decimal ApiVersion
    {
        get => apiVersion > 0 ? apiVersion : DefaultVersion;
        set => apiVersion = value;
    }

    public bool IsValid() => !string.IsNullOrWhiteSpace(SalesForceUrl) && !string.IsNullOrWhiteSpace(ClientId) &&
                             !string.IsNullOrWhiteSpace(ClientSecret);
}
