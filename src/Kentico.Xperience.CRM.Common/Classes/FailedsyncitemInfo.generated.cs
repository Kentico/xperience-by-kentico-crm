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
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FailedSyncItemInfoProvider), OBJECT_TYPE, "KenticoCRMCommon.FailedSyncItem", "FailedSyncItemID", "FailedSyncItemLastModified", null, null, null, null, null, null)
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
        /// Failed sync item entity class.
        /// </summary>
        [DatabaseField]
        public virtual string FailedSyncItemEntityClass
        {
            get => ValidationHelper.GetString(GetValue(nameof(FailedSyncItemEntityClass)), String.Empty);
            set => SetValue(nameof(FailedSyncItemEntityClass), value);
        }


        /// <summary>
        /// Failed sync item entity ID.
        /// </summary>
        [DatabaseField]
        public virtual int FailedSyncItemEntityID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FailedSyncItemEntityID)), 0);
            set => SetValue(nameof(FailedSyncItemEntityID), value);
        }


        /// <summary>
        /// Failed sync item entity CRM.
        /// </summary>
        [DatabaseField]
        public virtual string FailedSyncItemEntityCRM
        {
            get => ValidationHelper.GetString(GetValue(nameof(FailedSyncItemEntityCRM)), String.Empty);
            set => SetValue(nameof(FailedSyncItemEntityCRM), value);
        }


        /// <summary>
        /// Failed sync item try count.
        /// </summary>
        [DatabaseField]
        public virtual int FailedSyncItemTryCount
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FailedSyncItemTryCount)), 0);
            set => SetValue(nameof(FailedSyncItemTryCount), value);
        }


        /// <summary>
        /// Failed sync item next time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FailedSyncItemNextTime
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(FailedSyncItemNextTime)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(FailedSyncItemNextTime), value);
        }


        /// <summary>
        /// Failed sync item last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FailedSyncItemLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(FailedSyncItemLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(FailedSyncItemLastModified), value);
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