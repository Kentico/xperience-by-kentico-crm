using CMS.ContactManagement;

namespace Kentico.Xperience.CRM.Common.Mapping.Resolvers;

/// <summary>
/// Produces a CRM-friendly value from a contact field which stores a reference or a coded value
/// rather than text - for example <see cref="ContactInfo.ContactCountryID"/>.
/// Resolvers are offered to marketers as a picker in the field mapping UI.
/// </summary>
public interface IContactSourceValueResolver
{
    /// <summary>
    /// Stable identifier persisted in the mapping configuration.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Name shown in the admin UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Contact field the resolver reads when the mapping does not specify one.
    /// </summary>
    string DefaultSourceField { get; }

    /// <summary>
    /// Resolves the value. Returns <see langword="null"/> when nothing can be resolved.
    /// </summary>
    /// <param name="contactInfo">Contact being synchronized.</param>
    /// <param name="sourceField">Contact field to read, or <see langword="null"/> to use <see cref="DefaultSourceField"/>.</param>
    object? Resolve(ContactInfo contactInfo, string? sourceField);
}