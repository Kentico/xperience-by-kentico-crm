using CMS.DataEngine;

namespace Kentico.Xperience.CRM.Common.Classes
{
    /// <summary>
    /// Class providing <see cref="CRMSyncItemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(ICRMSyncItemInfoProvider))]
    public partial class CRMSyncItemInfoProvider : AbstractInfoProvider<CRMSyncItemInfo, CRMSyncItemInfoProvider>, ICRMSyncItemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CRMSyncItemInfoProvider"/> class.
        /// </summary>
        public CRMSyncItemInfoProvider()
            : base(CRMSyncItemInfo.TYPEINFO)
        {
        }
    }
}