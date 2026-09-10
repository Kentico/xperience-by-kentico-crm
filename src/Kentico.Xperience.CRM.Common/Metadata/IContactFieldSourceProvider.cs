namespace Kentico.Xperience.CRM.Common.Metadata;

/// <summary>
/// Enumerates the Xperience contact fields available as mapping sources, including fields added
/// to the contact class by the project.
/// </summary>
public interface IContactFieldSourceProvider
{
    /// <summary>
    /// Returns the contact fields ordered by display name.
    /// </summary>
    IReadOnlyList<ContactFieldMetadata> GetContactFields();
}