using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Synchronization;

/// <summary>
/// Validation service interface for validating BizForm item before sending to CRM
/// </summary>
public interface ILeadsIntegrationValidationService
{
    /// <summary>
    /// Validates BizForm item to get result if given BizForm item can be sent
    /// </summary>
    /// <param name="bizFormItem"></param>
    /// <returns></returns>
    Task<bool> ValidateFormItem(BizFormItem bizFormItem);
}