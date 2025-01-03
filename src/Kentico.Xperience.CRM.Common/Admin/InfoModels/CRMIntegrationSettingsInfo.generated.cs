using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.CRM.Common;

[assembly: RegisterObjectType(typeof(CRMIntegrationSettingsInfo), CRMIntegrationSettingsInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.CRM.Common
{
    /// <summary>
    /// Data container class for <see cref="CRMIntegrationSettingsInfo"/>.
    /// </summary>
    [Serializable]
    public partial class CRMIntegrationSettingsInfo : AbstractInfo<CRMIntegrationSettingsInfo, ICRMIntegrationSettingsInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticocrmcommon.crmintegrationsettings";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CRMIntegrationSettingsInfoProvider), OBJECT_TYPE, "KenticoCRMCommon.CRMIntegrationSettings", "CRMIntegrationSettingsItemID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// CRM integration settings item ID.
        /// </summary>
        [DatabaseField]
        public virtual int CRMIntegrationSettingsItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CRMIntegrationSettingsItemID)), 0);
            set => SetValue(nameof(CRMIntegrationSettingsItemID), value);
        }


        /// <summary>
        /// CRM integration settings forms enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool CRMIntegrationSettingsFormsEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CRMIntegrationSettingsFormsEnabled)), false);
            set => SetValue(nameof(CRMIntegrationSettingsFormsEnabled), value);
        }


        /// <summary>
        /// CRM integration settings contacts enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool CRMIntegrationSettingsContactsEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CRMIntegrationSettingsContactsEnabled)), false);
            set => SetValue(nameof(CRMIntegrationSettingsContactsEnabled), value);
        }
        
        /// <summary>
        /// When true, data are synced from CRM to Kentico. Relevant only when <see cref="CRMIntegrationSettingsContactsEnabled"/> is true.
        /// </summary>
        [DatabaseField]
        public virtual bool CRMIntegrationSettingsContactsTwoWaySyncEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CRMIntegrationSettingsContactsTwoWaySyncEnabled)), true);
            set => SetValue(nameof(CRMIntegrationSettingsContactsTwoWaySyncEnabled), value);
        }
        
        /// <summary>
        /// CRM integration settings ignore existing records.
        /// </summary>
        [DatabaseField]
        public virtual bool CRMIntegrationSettingsIgnoreExistingRecords
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(CRMIntegrationSettingsIgnoreExistingRecords)), false);
            set => SetValue(nameof(CRMIntegrationSettingsIgnoreExistingRecords), value);
        }


        /// <summary>
        /// CRM integration settings url.
        /// </summary>
        [DatabaseField]
        public virtual string CRMIntegrationSettingsUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMIntegrationSettingsUrl)), String.Empty);
            set => SetValue(nameof(CRMIntegrationSettingsUrl), value);
        }


        /// <summary>
        /// CRM integration settings client id.
        /// </summary>
        [DatabaseField]
        public virtual string CRMIntegrationSettingsClientId
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMIntegrationSettingsClientId)), String.Empty);
            set => SetValue(nameof(CRMIntegrationSettingsClientId), value);
        }


        /// <summary>
        /// CRM integration settings client secret.
        /// </summary>
        [DatabaseField]
        public virtual string CRMIntegrationSettingsClientSecret
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMIntegrationSettingsClientSecret)), String.Empty);
            set => SetValue(nameof(CRMIntegrationSettingsClientSecret), value);
        }


        /// <summary>
        /// CRM integration settings CRM type.
        /// </summary>
        [DatabaseField]
        public virtual string CRMIntegrationSettingsCRMName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CRMIntegrationSettingsCRMName)), String.Empty);
            set => SetValue(nameof(CRMIntegrationSettingsCRMName), value);
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
        /// Creates an empty instance of the <see cref="CRMIntegrationSettingsInfo"/> class.
        /// </summary>
        public CRMIntegrationSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CRMIntegrationSettingsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CRMIntegrationSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}