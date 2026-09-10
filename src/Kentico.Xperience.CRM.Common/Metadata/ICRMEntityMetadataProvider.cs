namespace Kentico.Xperience.CRM.Common.Metadata;

/// <summary>
/// Supplies the field metadata of a CRM entity for the field mapping UI. Implementations retrieve the
/// live schema from the CRM and fall back to the compiled entity classes when that is not possible.
/// </summary>
public interface ICRMEntityMetadataProvider
{
    /// <summary>
    /// CRM the provider describes. See <see cref="Constants.CRMType"/>.
    /// </summary>
    string CRMName { get; }

    /// <summary>
    /// Returns the writable fields of the given entity type. Never throws for an unreachable CRM -
    /// the fallback metadata is returned with <see cref="CRMEntityMetadata.IsLive"/> set to <see langword="false"/>.
    /// </summary>
    /// <param name="entityType">Entity type. See <see cref="Constants.EntityType"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CRMEntityMetadata> GetEntityMetadataAsync(string entityType,
        CancellationToken cancellationToken = default);
}