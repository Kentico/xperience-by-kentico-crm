using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

internal class LeadIntegrationValidationService : ILeadsIntegrationValidationService
{
    public Task<bool> ValidateFormItem(BizFormItem bizFormItem) => Task.FromResult(true);
}