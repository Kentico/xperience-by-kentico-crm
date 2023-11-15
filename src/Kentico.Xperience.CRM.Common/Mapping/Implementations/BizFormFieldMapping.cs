namespace Kentico.Xperience.CRM.Common.Mapping.Implementations;

/// <summary>
/// Mapping wrapper for BizForm field mapping and Crm entity field mapping
/// </summary>
public class BizFormFieldMapping
{
    public BizFormFieldMapping(IBizFormFieldMapping formFieldMapping, ICrmFieldMapping crmFieldMapping)
    {
        FormFieldMapping = formFieldMapping;
        CrmFieldMapping = crmFieldMapping;
    }
    public IBizFormFieldMapping FormFieldMapping { get; }
    public ICrmFieldMapping CrmFieldMapping { get; }
}