namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Mapping wrapper for BizForm field mapping and Crm entity field mapping
/// </summary>
public class BizFormFieldMapping
{
    public BizFormFieldMapping(IBizFormFieldMapping formFieldMapping, ICRMFieldMapping crmFieldMapping)
    {
        FormFieldMapping = formFieldMapping;
        CRMFieldMapping = crmFieldMapping;
    }
    public IBizFormFieldMapping FormFieldMapping { get; }
    public ICRMFieldMapping CRMFieldMapping { get; }
}