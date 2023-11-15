using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Services.Implementations;

internal class LeadIntegrationValidationService : ILeadsIntegrationValidationService
{
    public bool ValidateFormItem(BizFormItem bizFormItem) => true;
}