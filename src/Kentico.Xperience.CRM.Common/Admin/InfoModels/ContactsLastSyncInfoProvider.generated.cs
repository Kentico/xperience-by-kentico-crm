using CMS.DataEngine;

namespace Kentico.Xperience.CRM.Common
{
    /// <summary>
    /// Class providing <see cref="ContactsLastSyncInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IContactsLastSyncInfoProvider))]
    public partial class ContactsLastSyncInfoProvider : AbstractInfoProvider<ContactsLastSyncInfo, ContactsLastSyncInfoProvider>, IContactsLastSyncInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactsLastSyncInfoProvider"/> class.
        /// </summary>
        public ContactsLastSyncInfoProvider()
            : base(ContactsLastSyncInfo.TYPEINFO)
        {
        }
    }
}