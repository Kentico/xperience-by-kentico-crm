using CMS.DataEngine;

namespace Kentico.Xperience.CRM.Common
{
    /// <summary>
    /// Class providing <see cref="FailedSyncItemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IFailedSyncItemInfoProvider))]
    public partial class FailedSyncItemInfoProvider : AbstractInfoProvider<FailedSyncItemInfo, FailedSyncItemInfoProvider>, IFailedSyncItemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedSyncItemInfoProvider"/> class.
        /// </summary>
        public FailedSyncItemInfoProvider()
            : base(FailedSyncItemInfo.TYPEINFO)
        {
        }
    }
}