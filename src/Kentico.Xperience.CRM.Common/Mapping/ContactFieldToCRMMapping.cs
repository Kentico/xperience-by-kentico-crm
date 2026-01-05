namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Mapping wrapper for BizForm field mapping and Crm entity field mapping
/// </summary>
public class ContactFieldToCRMMapping
{
    public ContactFieldToCRMMapping(IContactFieldMapping contactFieldMapping, ICRMFieldMapping crmFieldMapping)
    {
        ContactFieldMapping = contactFieldMapping;
        CRMFieldMapping = crmFieldMapping;
    }
    public IContactFieldMapping ContactFieldMapping { get; }
    public ICRMFieldMapping CRMFieldMapping { get; }
}