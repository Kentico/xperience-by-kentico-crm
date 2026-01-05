using CMS.Helpers;

namespace Kentico.Xperience.CRM.Common.Configuration;

internal class CRMSettingsService : ICRMSettingsService
{
    private readonly ICRMIntegrationSettingsInfoProvider integrationSettingsInfoProvider;
    private readonly IProgressiveCache cache;

    public CRMSettingsService(ICRMIntegrationSettingsInfoProvider integrationSettingsInfoProvider, IProgressiveCache cache)
    {
        this.integrationSettingsInfoProvider = integrationSettingsInfoProvider;
        this.cache = cache;
    }


    public CRMIntegrationSettingsInfo? GetSettings(string crmName) =>
        cache.Load(cs => integrationSettingsInfoProvider
                .Get()
                .WhereEquals(nameof(CRMIntegrationSettingsInfo.CRMIntegrationSettingsCRMName), crmName)
                .TopN(1)
                .FirstOrDefault()
            , new CacheSettings(20, $"{nameof(CRMSettingsService)}|{crmName}")
            {
                CacheDependency = CacheHelper.GetCacheDependency($"{CRMIntegrationSettingsInfo.OBJECT_TYPE}|all")
            });
}