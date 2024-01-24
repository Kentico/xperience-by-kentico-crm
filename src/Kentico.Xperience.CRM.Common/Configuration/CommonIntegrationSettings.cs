namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common setting for Kentico-CRM integration
/// </summary>
/// <typeparam name="TApiConfig"></typeparam>
public class CommonIntegrationSettings<TApiConfig> where TApiConfig : new()
{
    /// <summary>
    /// If enabled BizForm leads synchronization
    /// </summary>
    public bool FormLeadsEnabled { get; set; }

    public bool ContactsEnabled { get; set; }

    /// <summary>
    /// If true no existing item with same email or paired record by ID is updated
    /// </summary>
    public bool IgnoreExistingRecords { get; set; }

    /// <summary>
    /// Specific CRM API configuration
    /// </summary>
    public TApiConfig ApiConfig { get; set; } = new();
}