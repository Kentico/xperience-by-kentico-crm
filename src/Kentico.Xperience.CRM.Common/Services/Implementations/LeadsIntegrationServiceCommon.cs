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
    public async Task SynchronizeLeadAsync(BizFormItem bizFormItem)
    {
        if (!await validationService.ValidateFormItem(bizFormItem))
        {
            logger.LogInformation("BizForm item {ItemID} for {BizFormDisplayName} refused by validation",
                bizFormItem.ItemID, bizFormItem.BizFormInfo.FormDisplayName);
            return;
        }

        if (bizFormMappingConfig.FormsMappings.TryGetValue(bizFormItem.BizFormClassName.ToLowerInvariant(),
                out var formMapping))
        {
            await SynchronizeLeadAsync(bizFormItem, formMapping);
        }
    }
    
    protected abstract Task SynchronizeLeadAsync(BizFormItem bizFormItem, IEnumerable<BizFormFieldMapping> fieldMappings);
}