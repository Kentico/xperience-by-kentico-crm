using CMS.ContactManagement;
using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Mapping.Implementations;

/// <summary>
/// Contact Info item field mapping based on form field name
/// </summary>
public class ContactFieldNameMapping : IContactFieldMapping
{
    private readonly string contactFieldName;

    public ContactFieldNameMapping(string contactFieldName)
    {
        this.contactFieldName = contactFieldName;
    }

    public object MapContactField(ContactInfo contactInfo)
        => contactInfo.GetValue(contactFieldName);
}