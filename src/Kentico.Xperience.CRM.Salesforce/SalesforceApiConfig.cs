namespace Kentico.Xperience.CRM.Salesforce.Configuration;

/// <summary>
/// API config for Salesforce API
/// </summary>
public class SalesforceApiConfig
{
    public const decimal DefaultVersion = 59m;

    public string? SalesforceUrl { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    private decimal apiVersion;
    public decimal ApiVersion
    {
        get => apiVersion > 0 ? apiVersion : DefaultVersion;
        set => apiVersion = value;
    }

    public bool IsValid() => !string.IsNullOrWhiteSpace(SalesforceUrl) && !string.IsNullOrWhiteSpace(ClientId) &&
                             !string.IsNullOrWhiteSpace(ClientSecret);
}