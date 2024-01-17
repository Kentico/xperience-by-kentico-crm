using Kentico.Xperience.CRM.Common.Mapping.Implementations;

namespace Kentico.Xperience.CRM.Common.Configuration;

public class ContactMappingConfiguration
{
    public List<ContactFieldToCRMMapping> FieldsMapping { get; init; } = new();
    public List<Type> Converters { get; init; } = new();
}