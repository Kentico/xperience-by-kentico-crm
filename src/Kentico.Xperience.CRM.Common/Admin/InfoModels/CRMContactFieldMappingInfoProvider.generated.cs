using CMS.DataEngine;

namespace Kentico.Xperience.CRM.Common
{
    /// <summary>
    /// Class providing <see cref="CRMContactFieldMappingInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(ICRMContactFieldMappingInfoProvider))]
    public partial class CRMContactFieldMappingInfoProvider : AbstractInfoProvider<CRMContactFieldMappingInfo, CRMContactFieldMappingInfoProvider>, ICRMContactFieldMappingInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CRMContactFieldMappingInfoProvider"/> class.
        /// </summary>
        public CRMContactFieldMappingInfoProvider()
            : base(CRMContactFieldMappingInfo.TYPEINFO)
        {
        }
    }
}
