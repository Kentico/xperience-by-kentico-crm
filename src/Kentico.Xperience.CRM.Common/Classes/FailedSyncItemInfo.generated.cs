using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.CRM.Common.Classes;

[assembly: RegisterObjectType(typeof(FailedSyncItemInfo), FailedSyncItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.CRM.Common.Classes
{
    /// <summary>
    /// Data container class for <see cref="FailedSyncItemInfo"/>.
    /// </summary>
    [Serializable]
    public partial class FailedSyncItemInfo : AbstractInfo<FailedSyncItemInfo, IFailedSyncItemInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticocrmcommon.failedsyncitem";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FailedSyncItemInfoProvider), OBJECT_TYPE, "kenticocrmcommon.FailedSyncItem", "FailedSyncItemID", "SyncLastModified", null, null, null, null, null, null)
        {
            ModuleName = "Kentic.Xperience.CRM.Common",
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Failed sync item ID.
        /// </summary>
        [DatabaseField]
        public virtual int FailedSyncItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FailedSyncItemID)), 0);
            set => SetValue(nameof(FailedSyncItemID), value);
        }


        /// <summary>
        /// Entity type.
        /// </summary>
        [DatabaseField]
        public virtual string EntityType
        {
            get => ValidationHelper.GetString(GetValue(nameof(EntityType)), String.Empty);
            set => SetValue(nameof(EntityType), value);
        }


        /// <summary>
        /// Entity class.
        /// </summary>
        [DatabaseField]
        public virtual string EntityClass
        {
            get => ValidationHelper.GetString(GetValue(nameof(EntityClass)), String.Empty);
            set => SetValue(nameof(EntityClass), value);
        }


        /// <summary>
        /// Entity ID.
        /// </summary>
        [DatabaseField]
        public virtual int EntityID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EntityID)), 0);
            set => SetValue(nameof(EntityID), value);
        }


        /// <summary>
        /// Entity CRM.
        /// </summary>
        [DatabaseField]
        public virtual string EntityCRM
        {
            get => ValidationHelper.GetString(GetValue(nameof(EntityCRM)), String.Empty);
            set => SetValue(nameof(EntityCRM), value);
        }


        /// <summary>
        /// Sync try count.
        /// </summary>
        [DatabaseField]
        public virtual int SyncTryCount
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SyncTryCount)), 0);
            set => SetValue(nameof(SyncTryCount), value);
        }


        /// <summary>
        /// Sync next time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SyncNextTime
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SyncNextTime)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SyncNextTime), value);
        }


        /// <summary>
        /// Sync last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SyncLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SyncLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SyncLastModified), value);
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
        protected FailedSyncItemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="FailedSyncItemInfo"/> class.
        /// </summary>
        public FailedSyncItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FailedSyncItemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FailedSyncItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}