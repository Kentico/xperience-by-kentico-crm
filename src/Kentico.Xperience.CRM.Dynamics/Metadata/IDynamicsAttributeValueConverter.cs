namespace Kentico.Xperience.CRM.Dynamics.Metadata;

/// <summary>
/// Converts a mapped value to the CLR type a Dataverse attribute expects.
/// Mappings configured in the admin UI produce text, while Dataverse rejects text for an option set,
/// a money or a lookup attribute, so the value has to be converted before it is written.
/// </summary>
internal interface IDynamicsAttributeValueConverter
{
    /// <summary>
    /// Converts <paramref name="value"/> for the given attribute. Values which already have the expected
    /// type are returned unchanged, and a value which cannot be converted is returned unchanged as well so
    /// that the CRM reports the problem rather than this converter silently dropping data.
    /// </summary>
    /// <param name="entityLogicalName">Dataverse entity logical name, for example <c>lead</c>.</param>
    /// <param name="attributeName">Attribute logical name.</param>
    /// <param name="value">Value produced by the mapping.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<object?> ConvertAsync(string entityLogicalName, string attributeName, object? value,
        CancellationToken cancellationToken = default);
}