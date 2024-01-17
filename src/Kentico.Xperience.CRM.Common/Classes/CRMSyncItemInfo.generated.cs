using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.CRM.Common.Classes;

[assembly: RegisterObjectType(typeof(CRMSyncItemInfo), CRMSyncItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.CRM.Common.Classes
{
    /// <summary>
    /// Data container class for <see cref="CRMSyncItemInfo"/>.
    /// </summary>
    [Serializable]
    public partial class CRMSyncItemInfo : AbstractInfo<CRMSyncItemInfo, ICRMSyncItemInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticocrmcommon.crmsyncitem";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CRMSyncItemInfoProvider), OBJECT_TYPE, "KenticoCRMCommon.CRMSyncItem", "CRMSyncItemID", "CRMSyncItemLastModified", null, null, null, null, null, null)
        {
            ModuleName = "Kentic.Xperience.CRM.Common",
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// CRM sync item ID.
        /// </summary>
        [DatabaseField]
        public virtual int CRMSyncItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CRMSyncItemID)), 0);
            set => SetValue(nameof(CRMSyncItemID), value);
        }


        /// <summary>
        /// CRM sync item entity class.
        /// </summary>
        [DatabaseField]
        public virtual string CRMSyncItemEntityClass
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMSyncItemEntityClass)), String.Empty);
            set => SetValue(nameof(CRMSyncItemEntityClass), value);
        }


        /// <summary>
        /// CRM sync item entity ID.
        /// </summary>
        [DatabaseField]
        public virtual string CRMSyncItemEntityID
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMSyncItemEntityID)), String.Empty);
            set => SetValue(nameof(CRMSyncItemEntityID), value);
        }


        /// <summary>
        /// CRM sync item CRMID.
        /// </summary>
        [DatabaseField]
        public virtual string CRMSyncItemCRMID
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMSyncItemCRMID)), String.Empty);
            set => SetValue(nameof(CRMSyncItemCRMID), value);
        }


        /// <summary>
        /// CRM sync item entity CRM.
        /// </summary>
        [DatabaseField]
        public virtual string CRMSyncItemEntityCRM
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMSyncItemEntityCRM)), String.Empty);
            set => SetValue(nameof(CRMSyncItemEntityCRM), value);
        }


        /// <summary>
        /// CRM sync item created by kentico.
        /// </summary>
        [DatabaseField]
        public virtual bool CRMSyncItemCreatedByKentico
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CRMSyncItemCreatedByKentico)), false);
            set => SetValue(nameof(CRMSyncItemCreatedByKentico), value);
        }


        /// <summary>
        /// CRM sync item last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CRMSyncItemLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CRMSyncItemLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CRMSyncItemLastModified), value);
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
        protected CRMSyncItemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="CRMSyncItemInfo"/> class.
        /// </summary>
        public CRMSyncItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CRMSyncItemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CRMSyncItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}