using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Configuration;
using Kentico.Xperience.CRM.Common.Mapping.Implementations;
using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

/// <summary>
/// This abstract class contains common functionality for specific Lead integration service like calling validation and
/// getting mapping for given BizForm item
/// </summary>
public abstract class LeadsIntegrationServiceCommon : ILeadsIntegrationService
{
    private readonly BizFormsMappingConfiguration bizFormMappingConfig;
    private readonly ILeadsIntegrationValidationService validationService;
    private readonly ILogger<LeadsIntegrationServiceCommon> logger;

    protected LeadsIntegrationServiceCommon(
        BizFormsMappingConfiguration bizFormMappingConfig,
        ILeadsIntegrationValidationService validationService,
        ILogger<LeadsIntegrationServiceCommon> logger)
    {
        this.bizFormMappingConfig = bizFormMappingConfig;
        this.validationService = validationService;
        this.logger = logger;
    }

    /// <summary>
    /// Validates BizForm item, then get specific mapping and finally specific implementation is called
    /// from inherited service
    /// </summary>
    /// <param name="bizFormItem"></param>
    public async Task CreateLeadAsync(BizFormItem bizFormItem)
    {
        if (!validationService.ValidateFormItem(bizFormItem))
        {
            logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} refused by validation",
                bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
            return;
        }

        if (bizFormMappingConfig.FormsMappings.TryGetValue(bizFormItem.BizFormClassName.ToLowerInvariant(),
                out var formMapping))
        {
            await CreateLeadAsync(bizFormItem, formMapping);
        }
    }

    /// <summary>
    /// Validates BizForm item, then get specific mapping and finally specific implementation is called
    /// from inherited service
    /// </summary>
    /// <param name="bizFormItem"></param>
    public async Task UpdateLeadAsync(BizFormItem bizFormItem)
    {
        if (!validationService.ValidateFormItem(bizFormItem))
        {
            logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} refused by validation",
                bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
            return;
        }

        if (bizFormMappingConfig.FormsMappings.TryGetValue(bizFormItem.BizFormClassName.ToLowerInvariant(),
                out var formMapping))
        {
            await UpdateLeadAsync(bizFormItem, formMapping);
        }
    }

    protected abstract Task CreateLeadAsync(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings);
    protected abstract Task UpdateLeadAsync(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings);

    protected virtual string FormatExternalId(BizFormItem bizFormItem) =>
        $"{bizFormItem.BizFormClassName}-{bizFormItem.ItemID}";
}