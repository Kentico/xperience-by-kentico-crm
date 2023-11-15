using CMS.OnlineForms;
using Kentico.Xperience.CRM.Common.Services;

namespace DancingGoat.Services.CRM;

public class CustomFormLeadsValidationService : ILeadsIntegrationValidationService
{
    public bool ValidateFormItem(BizFormItem bizFormItem)
    {
        return true;
    }
}
