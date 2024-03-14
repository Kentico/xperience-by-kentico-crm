using Kentico.Xperience.CRM.Common.Mapping;

namespace Kentico.Xperience.CRM.Common.Configuration;

public class ContactMappingConfiguration
{
    public List<ContactFieldToCRMMapping> FieldsMapping { get; init; } = new();
}