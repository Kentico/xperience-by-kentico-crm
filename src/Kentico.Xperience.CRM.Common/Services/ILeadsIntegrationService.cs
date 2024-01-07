using CMS.ContactManagement;
using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Services;

/// <summary>
/// Common service interface for sending BizForm item to CRM
/// </summary>
public interface ILeadsIntegrationService
{
    /// <summary>
    /// Creates lead in CRM from BizForm item
    /// </summary>
    /// <param name="bizFormItem"></param>
    /// <returns></returns>
    Task CreateLeadAsync(BizFormItem bizFormItem);

    /// <summary>
    /// Updates lead in CRM from BizForm item
    /// </summary>
    /// <param name="bizFormItem"></param>
    /// <returns></returns>
    Task UpdateLeadAsync(BizFormItem bizFormItem);
}