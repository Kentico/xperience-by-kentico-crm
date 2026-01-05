using CMS.ContactManagement;

namespace Kentico.Xperience.CRM.Common.Mapping;

public class ContactFieldMappingFunction : IContactFieldMapping
{
    private readonly Func<ContactInfo, object> mappingFunc;

    public ContactFieldMappingFunction(Func<ContactInfo, object> mappingFunc)
    {
        this.mappingFunc = mappingFunc;
    }

    public object MapContactField(ContactInfo contactInfo) => mappingFunc(contactInfo);
}