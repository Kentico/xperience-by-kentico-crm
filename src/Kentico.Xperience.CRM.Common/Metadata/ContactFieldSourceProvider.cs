using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace Kentico.Xperience.CRM.Common.Metadata;

/// <summary>
/// Reads the contact fields from the form definition of the contact class, so that contact fields added
/// by the project are offered as mapping sources alongside the system ones.
/// </summary>
internal class ContactFieldSourceProvider : IContactFieldSourceProvider
{
    private readonly IProgressiveCache cache;

    public ContactFieldSourceProvider(IProgressiveCache cache) => this.cache = cache;

    public IReadOnlyList<ContactFieldMetadata> GetContactFields() => cache.Load(_ => LoadContactFields(),
        new CacheSettings(20, $"{nameof(ContactFieldSourceProvider)}|fields")
        {
            CacheDependency = CacheHelper.GetCacheDependency($"{DataClassInfo.OBJECT_TYPE}|byname|{ContactInfo.TYPEINFO.ObjectClassName}")
        });

    private static List<ContactFieldMetadata> LoadContactFields()
    {
        var dataClass = DataClassInfoProvider.GetDataClassInfo(ContactInfo.TYPEINFO.ObjectClassName);

        if (dataClass is null)
        {
            return new List<ContactFieldMetadata>();
        }

        var formInfo = new FormInfo(dataClass.ClassFormDefinition);

        return formInfo
            .GetFields(visible: true, invisible: true, includeSystem: true, onlyPrimaryKeys: false,
                includeDummyFields: false)
            .Where(f => !f.PrimaryKey)
            .Select(f => new ContactFieldMetadata
            {
                Name = f.Name,
                DisplayName = string.IsNullOrWhiteSpace(f.Caption) ? f.Name : f.Caption,
                DataType = f.DataType
            })
            .OrderBy(f => f.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}