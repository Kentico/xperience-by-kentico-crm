using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.CRM.Common;

[assembly: RegisterObjectType(typeof(CRMContactFieldMappingInfo), CRMContactFieldMappingInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.CRM.Common
{
    /// <summary>
    /// Data container class for <see cref="CRMContactFieldMappingInfo"/>.
    /// </summary>
    [Serializable]
    public partial class CRMContactFieldMappingInfo : AbstractInfo<CRMContactFieldMappingInfo, ICRMContactFieldMappingInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticocrmcommon.crmcontactfieldmapping";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CRMContactFieldMappingInfoProvider), OBJECT_TYPE, "KenticoCRMCommon.CRMContactFieldMapping", "CRMContactFieldMappingID", "CRMContactFieldMappingLastModified", "CRMContactFieldMappingGUID", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// CRM contact field mapping ID.
        /// </summary>
        [DatabaseField]
        public virtual int CRMContactFieldMappingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CRMContactFieldMappingID)), 0);
            set => SetValue(nameof(CRMContactFieldMappingID), value);
        }


        /// <summary>
        /// Name of the CRM the mapping belongs to. See <see cref="Constants.CRMType"/>.
        /// </summary>
        [DatabaseField]
        public virtual string CRMContactFieldMappingCRMName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMContactFieldMappingCRMName)), String.Empty);
            set => SetValue(nameof(CRMContactFieldMappingCRMName), value);
        }


        /// <summary>
        /// Target CRM entity type the mapping belongs to. See <see cref="Constants.EntityType"/>.
        /// </summary>
        [DatabaseField]
        public virtual string CRMContactFieldMappingEntityType
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMContactFieldMappingEntityType)), String.Empty);
            set => SetValue(nameof(CRMContactFieldMappingEntityType), value);
        }


        /// <summary>
        /// Name of the CRM field the value is written to.
        /// </summary>
        [DatabaseField]
        public virtual string CRMContactFieldMappingCRMField
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMContactFieldMappingCRMField)), String.Empty);
            set => SetValue(nameof(CRMContactFieldMappingCRMField), value);
        }


        /// <summary>
        /// Kind of the value expression. See <see cref="Mapping.ContactFieldValueKind"/>.
        /// </summary>
        [DatabaseField]
        public virtual string CRMContactFieldMappingValueKind
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMContactFieldMappingValueKind)), String.Empty);
            set => SetValue(nameof(CRMContactFieldMappingValueKind), value);
        }


        /// <summary>
        /// Serialized <see cref="Mapping.ContactFieldValueExpression"/>.
        /// </summary>
        [DatabaseField]
        public virtual string CRMContactFieldMappingExpression
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMContactFieldMappingExpression)), String.Empty);
            set => SetValue(nameof(CRMContactFieldMappingExpression), value);
        }


        /// <summary>
        /// Order of the mapping within its CRM and entity type.
        /// </summary>
        [DatabaseField]
        public virtual int CRMContactFieldMappingOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CRMContactFieldMappingOrder)), 0);
            set => SetValue(nameof(CRMContactFieldMappingOrder), value);
        }


        /// <summary>
        /// Indicates whether the mapping is applied during synchronization.
        /// </summary>
        [DatabaseField]
        public virtual bool CRMContactFieldMappingEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CRMContactFieldMappingEnabled)), true);
            set => SetValue(nameof(CRMContactFieldMappingEnabled), value);
        }


        /// <summary>
        /// CRM contact field mapping GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid CRMContactFieldMappingGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(CRMContactFieldMappingGUID)), Guid.Empty);
            set => SetValue(nameof(CRMContactFieldMappingGUID), value);
        }


        /// <summary>
        /// CRM contact field mapping last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CRMContactFieldMappingLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CRMContactFieldMappingLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CRMContactFieldMappingLastModified), value);
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
        /// Creates an empty instance of the <see cref="CRMContactFieldMappingInfo"/> class.
        /// </summary>
        public CRMContactFieldMappingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CRMContactFieldMappingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CRMContactFieldMappingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
