using CMS.ContactManagement;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;

namespace Kentico.Xperience.CRM.Common.Configuration;

public class ContactMappingBuilder
{
    private List<ContactFieldToCRMMapping> fieldMappings = new();
    
    public ContactMappingBuilder MapField(string contactFieldName, string crmFieldName)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldNameMapping(contactFieldName), new CRMFieldNameMapping(crmFieldName)));
        return this;
    }

    public ContactMappingBuilder MapField(Func<ContactInfo, object> mappingFunc, string crmFieldName)
    {
        fieldMappings.Add(new ContactFieldToCRMMapping(new ContactFieldMappingFunction(mappingFunc), new CRMFieldNameMapping(crmFieldName)));
        return this;
    }
    
    internal TContactMappingConfiguration Build<TContactMappingConfiguration>()
        where TContactMappingConfiguration : ContactMappingConfiguration, new()
    {
        return new TContactMappingConfiguration
        {
            FieldsMapping = fieldMappings
        };
    }
}