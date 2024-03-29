﻿namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// CRM entity field mapping based on field name
/// </summary>
public class CRMFieldNameMapping : ICRMFieldMapping
{
    public CRMFieldNameMapping(string crmFieldName)
    {
        CrmFieldName = crmFieldName;
    }

    public string CrmFieldName { get; }
}