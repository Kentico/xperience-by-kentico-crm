namespace Kentico.Xperience.CRM.Common.Configuration;

/// <summary>
/// Common builder for BizForms to CRM leads configuration mapping 
/// </summary>
public class BizFormsMappingBuilder
{
    private readonly Dictionary<string, BizFormFieldsMappingBuilder> forms = new();
    private string? externalIdFieldName;

    public BizFormsMappingBuilder AddForm(string formCodeName,
        Func<BizFormFieldsMappingBuilder, BizFormFieldsMappingBuilder> configureFields)
    {
        if (formCodeName is null) throw new ArgumentNullException(nameof(formCodeName));

        forms.Add(formCodeName.ToLowerInvariant(), configureFields(new BizFormFieldsMappingBuilder()));
        return this;
    }

    public BizFormsMappingBuilder ExternalIdField(string fieldName)
    {
        externalIdFieldName = fieldName;
        return this;
    }

    internal TBizFormsConfiguration Build<TBizFormsConfiguration>()
     where TBizFormsConfiguration : BizFormsMappingConfiguration, new()
    {
        return new TBizFormsConfiguration
        {
            FormsMappings = forms.Select(f => (f.Key, f.Value.Build()))
                .ToDictionary(r => r.Key, r => r.Item2),
            ExternalIdFieldName = externalIdFieldName
        };
    }
}