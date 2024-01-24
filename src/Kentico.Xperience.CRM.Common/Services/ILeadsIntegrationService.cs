using CMS.OnlineForms;

namespace Kentico.Xperience.CRM.Common.Services;

/// <summary>
/// Common service interface for sending BizForm item to CRM
/// </summary>
public interface ILeadsIntegrationService
{
    /// <summary>
    /// Updates lead in CRM from BizForm item
    /// </summary>
    /// <param name="bizFormItem"></param>
    /// <returns></returns>
    Task SynchronizeLeadAsync(BizFormItem bizFormItem);
}