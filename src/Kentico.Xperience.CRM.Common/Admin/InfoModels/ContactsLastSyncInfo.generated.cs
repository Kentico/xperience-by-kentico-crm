using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.CRM.Common;

[assembly: RegisterObjectType(typeof(ContactsLastSyncInfo), ContactsLastSyncInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.CRM.Common
{
    /// <summary>
    /// Data container class for <see cref="ContactsLastSyncInfo"/>.
    /// </summary>
    [Serializable]
    public partial class ContactsLastSyncInfo : AbstractInfo<ContactsLastSyncInfo, IContactsLastSyncInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticocrmcommon.contactslastsync";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactsLastSyncInfoProvider), OBJECT_TYPE, "KenticoCRMCommon.ContactsLastSync", "ContactsLastSyncItemID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Contacts last sync item ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContactsLastSyncItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContactsLastSyncItemID)), 0);
            set => SetValue(nameof(ContactsLastSyncItemID), value);
        }


        /// <summary>
        /// Contacts last sync CRM.
        /// </summary>
        [DatabaseField]
        public virtual string ContactsLastSyncCRM
        {
            get => ValidationHelper.GetString(GetValue(nameof(ContactsLastSyncCRM)), String.Empty);
            set => SetValue(nameof(ContactsLastSyncCRM), value);
        }


        /// <summary>
        /// Contacts last sync time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ContactsLastSyncTime
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ContactsLastSyncTime)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ContactsLastSyncTime), value);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ContactsLastSyncInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ContactsLastSyncInfo"/> class.
        /// </summary>
        public ContactsLastSyncInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ContactsLastSyncInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContactsLastSyncInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}