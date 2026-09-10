using CMS.ContactManagement;
using CMS.Globalization;
using CMS.Helpers;

namespace Kentico.Xperience.CRM.Common.Mapping.Resolvers;

/// <summary>
/// Resolves a state ID stored on the contact to the state display name.
/// </summary>
public class StateNameResolver : IContactSourceValueResolver
{
    public const string ResolverName = "state";

    public string Name => ResolverName;

    public string DisplayName => "State name";

    public string DefaultSourceField => nameof(ContactInfo.ContactStateID);

    public object? Resolve(ContactInfo contactInfo, string? sourceField)
    {
        int stateId = ValidationHelper.GetInteger(
            contactInfo.GetValue(string.IsNullOrWhiteSpace(sourceField) ? DefaultSourceField : sourceField), 0);

        return stateId > 0 ? StateInfo.Provider.Get(stateId)?.StateDisplayName : null;
    }
}