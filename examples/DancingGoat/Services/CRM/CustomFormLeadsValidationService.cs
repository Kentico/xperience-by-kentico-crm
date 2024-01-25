using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Synchronization;

namespace DancingGoat.Services.CRM;

public class CustomFormLeadsValidationService : ILeadsIntegrationValidationService
{
    public Task<bool> ValidateFormItem(BizFormItem bizFormItem)
    {
        return Task.FromResult(true);
    }
}
