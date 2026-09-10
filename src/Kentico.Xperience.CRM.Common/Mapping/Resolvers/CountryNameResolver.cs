using CMS.ContactManagement;
using CMS.Globalization;
using CMS.Helpers;

namespace Kentico.Xperience.CRM.Common.Mapping.Resolvers;

/// <summary>
/// Resolves a country ID stored on the contact to the country display name.
/// </summary>
public class CountryNameResolver : IContactSourceValueResolver
{
    public const string ResolverName = "country";

    public string Name => ResolverName;

    public string DisplayName => "Country name";

    public string DefaultSourceField => nameof(ContactInfo.ContactCountryID);

    public object? Resolve(ContactInfo contactInfo, string? sourceField)
    {
        int countryId = ValidationHelper.GetInteger(
            contactInfo.GetValue(string.IsNullOrWhiteSpace(sourceField) ? DefaultSourceField : sourceField), 0);

        return countryId > 0 ? CountryInfo.Provider.Get(countryId)?.CountryDisplayName : null;
    }
}