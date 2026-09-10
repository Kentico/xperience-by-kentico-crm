using CMS.ContactManagement;
using CMS.Helpers;

namespace Kentico.Xperience.CRM.Common.Mapping.Resolvers;

/// <summary>
/// Resolves the numeric contact gender to a label. Xperience stores 0 - undefined, 1 - male, 2 - female.
/// </summary>
public class GenderLabelResolver : IContactSourceValueResolver
{
    public const string ResolverName = "gender";

    public string Name => ResolverName;

    public string DisplayName => "Gender label";

    public string DefaultSourceField => nameof(ContactInfo.ContactGender);

    public object? Resolve(ContactInfo contactInfo, string? sourceField)
    {
        int gender = ValidationHelper.GetInteger(
            contactInfo.GetValue(string.IsNullOrWhiteSpace(sourceField) ? DefaultSourceField : sourceField), 0);

        return gender switch
        {
            1 => "Male",
            2 => "Female",
            _ => null
        };
    }
}