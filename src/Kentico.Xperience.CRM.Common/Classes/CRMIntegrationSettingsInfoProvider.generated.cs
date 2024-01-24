using CMS.DataEngine;

namespace Kentico.Xperience.CRM.Common.Classes
{
    /// <summary>
    /// Class providing <see cref="CRMIntegrationSettingsInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(ICRMIntegrationSettingsInfoProvider))]
    public partial class CRMIntegrationSettingsInfoProvider : AbstractInfoProvider<CRMIntegrationSettingsInfo, CRMIntegrationSettingsInfoProvider>, ICRMIntegrationSettingsInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CRMIntegrationSettingsInfoProvider"/> class.
        /// </summary>
        public CRMIntegrationSettingsInfoProvider()
            : base(CRMIntegrationSettingsInfo.TYPEINFO)
        {
        }
    }
}