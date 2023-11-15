namespace Kentico.Xperience.CRM.Common.Mapping.Implementations;

/// <summary>
/// CRM entity field mapping based on field name
/// </summary>
public class CrmFieldNameMapping : ICrmFieldMapping
{
    public CrmFieldNameMapping(string crmFieldName)
    {
        CrmFieldName = crmFieldName;
    }

    public string CrmFieldName { get; }
}