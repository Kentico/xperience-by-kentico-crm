using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.CRM.Common.Mapping;
using Kentico.Xperience.CRM.Common.Mapping.Resolvers;

namespace Kentico.Xperience.CRM.Common.Configuration;

/// <inheritdoc />
internal class ContactFieldMappingService : IContactFieldMappingService
{
    private readonly ICRMContactFieldMappingInfoProvider mappingInfoProvider;
    private readonly IProgressiveCache cache;
    private readonly IReadOnlyDictionary<string, IContactSourceValueResolver> resolvers;

    public ContactFieldMappingService(ICRMContactFieldMappingInfoProvider mappingInfoProvider,
        IProgressiveCache cache,
        IEnumerable<IContactSourceValueResolver> resolvers)
    {
        this.mappingInfoProvider = mappingInfoProvider;
        this.cache = cache;

        // Grouped rather than indexed directly so that a duplicate registration cannot break startup.
        this.resolvers = resolvers
            .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ContactFieldMappingDefinition> GetDefinitions(string crmName, string entityType) =>
        GetInfos(crmName, entityType)
            .Select(i => new ContactFieldMappingDefinition
            {
                CRMField = i.CRMContactFieldMappingCRMField,
                Expression = ContactFieldValueExpression.FromJson(i.CRMContactFieldMappingExpression),
                Order = i.CRMContactFieldMappingOrder,
                Enabled = i.CRMContactFieldMappingEnabled
            })
            .ToList();

    public void SaveDefinitions(string crmName, string entityType,
        IEnumerable<ContactFieldMappingDefinition> definitions)
    {
        // The grid in the admin UI is the whole truth for a CRM and entity type, so the stored set is
        // replaced rather than merged. Wrapped in a transaction to avoid leaving a partial configuration
        // behind, which would silently change what gets synchronized.
        using var transaction = new CMSTransactionScope();

        foreach (var existing in GetInfos(crmName, entityType))
        {
            mappingInfoProvider.Delete(existing);
        }

        int order = 0;

        foreach (var definition in definitions.Where(d => !string.IsNullOrWhiteSpace(d.CRMField)))
        {
            var info = new CRMContactFieldMappingInfo
            {
                CRMContactFieldMappingCRMName = crmName,
                CRMContactFieldMappingEntityType = entityType,
                CRMContactFieldMappingCRMField = definition.CRMField,
                CRMContactFieldMappingValueKind = definition.Expression.Kind.ToString(),
                CRMContactFieldMappingExpression = definition.Expression.ToJson(),
                CRMContactFieldMappingOrder = order++,
                CRMContactFieldMappingEnabled = definition.Enabled
            };

            mappingInfoProvider.Set(info);
        }

        transaction.Commit();
    }

    public bool HasConfiguration(string crmName, string entityType) =>
        GetMappingSet(crmName, entityType).HasConfiguration;

    public IReadOnlyList<ContactFieldToCRMMapping> GetEffectiveMappings(string crmName, string entityType,
        IEnumerable<ContactFieldToCRMMapping> codeMappings)
    {
        var configured = GetMappingSet(crmName, entityType);

        return configured.HasConfiguration ? configured.Mappings : codeMappings.ToList();
    }

    public object Evaluate(ContactFieldValueExpression expression, ContactInfo contactInfo) =>
        new ExpressionContactFieldMapping(expression, resolvers).MapContactField(contactInfo);

    /// <summary>
    /// Loads the compiled configured mappings. Cached because this runs for every synchronized contact,
    /// and invalidated by the object type touch performed when a mapping is saved.
    /// </summary>
    private MappingSet GetMappingSet(string crmName, string entityType) =>
        cache.Load(_ => BuildMappingSet(crmName, entityType),
            new CacheSettings(20, $"{nameof(ContactFieldMappingService)}|{crmName}|{entityType}")
            {
                CacheDependency =
                    CacheHelper.GetCacheDependency($"{CRMContactFieldMappingInfo.OBJECT_TYPE}|all")
            });

    private MappingSet BuildMappingSet(string crmName, string entityType)
    {
        var infos = GetInfos(crmName, entityType);

        if (infos.Count == 0)
        {
            return MappingSet.NotConfigured;
        }

        // Disabled rows are deliberately dropped while still counting as a configuration: a marketer who
        // disables every row wants nothing mapped, not a silent fall back to the mappings defined in code.
        var mappings = infos
            .Where(i => i.CRMContactFieldMappingEnabled &&
                        !string.IsNullOrWhiteSpace(i.CRMContactFieldMappingCRMField))
            .Select(i => new ContactFieldToCRMMapping(
                new ExpressionContactFieldMapping(
                    ContactFieldValueExpression.FromJson(i.CRMContactFieldMappingExpression), resolvers),
                new CRMFieldNameMapping(i.CRMContactFieldMappingCRMField)))
            .ToList();

        return new MappingSet(true, mappings);
    }

    private List<CRMContactFieldMappingInfo> GetInfos(string crmName, string entityType) =>
        mappingInfoProvider.Get()
            .WhereEquals(nameof(CRMContactFieldMappingInfo.CRMContactFieldMappingCRMName), crmName)
            .WhereEquals(nameof(CRMContactFieldMappingInfo.CRMContactFieldMappingEntityType), entityType)
            .OrderBy(nameof(CRMContactFieldMappingInfo.CRMContactFieldMappingOrder))
            .ToList();

    /// <summary>
    /// Cacheable result which distinguishes "nothing configured" from "configured to map nothing".
    /// </summary>
    private sealed record MappingSet(bool HasConfiguration, IReadOnlyList<ContactFieldToCRMMapping> Mappings)
    {
        public static readonly MappingSet NotConfigured = new(false, Array.Empty<ContactFieldToCRMMapping>());
    }
}