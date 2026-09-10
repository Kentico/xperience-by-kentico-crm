namespace Kentico.Xperience.CRM.Common.Metadata;

/// <summary>
/// Field metadata of a single CRM entity together with the provenance of that metadata, so the admin UI
/// can tell marketers whether they are looking at their real CRM schema or at the compiled-in fallback.
/// </summary>
public class CRMEntityMetadata
{
    /// <summary>
    /// Entity type the metadata describes. See <see cref="Constants.EntityType"/>.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// <see langword="true"/> when the metadata was retrieved from the CRM, <see langword="false"/> when it
    /// was reflected from the compiled entity classes because the CRM could not be reached.
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Reason the live retrieval was not used. Shown to marketers as a warning.
    /// </summary>
    public string? Warning { get; set; }

    /// <summary>
    /// Writable fields of the entity, ordered by display name.
    /// </summary>
    public List<CRMFieldMetadata> Fields { get; set; } = new();
}