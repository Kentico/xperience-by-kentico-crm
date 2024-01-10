using Kentico.Xperience.CRM.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.CRM.SalesForce.Configuration;

public class SalesForceBizFormsMappingBuilder : BizFormsMappingBuilder
{
    public SalesForceBizFormsMappingBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }
    
    internal SalesForceBizFormsMappingConfiguration Build()
    {
        return new SalesForceBizFormsMappingConfiguration
        {
            FormsMappings = forms.Select(f => (f.Key, f.Value.Build()))
                .ToDictionary(r => r.Key, r => r.Item2),
        };
    }

}