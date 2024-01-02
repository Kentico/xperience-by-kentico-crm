using CMS.ContactManagement;

namespace Kentico.Xperience.CRM.Common.Mapping;

public interface IContactFieldMapping
{
    object MapContactField(ContactInfo contactInfo);
}