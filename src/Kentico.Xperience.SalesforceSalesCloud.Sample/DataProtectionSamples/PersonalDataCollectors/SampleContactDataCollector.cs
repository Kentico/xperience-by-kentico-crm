﻿using CMS.Activities;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Globalization;
using CMS.OnlineForms;

namespace Samples.DancingGoat
{
    /// <summary>
    /// Sample implementation of <see cref="IPersonalDataCollector"/> interface for collecting contact's personal data.
    /// </summary>
    internal class SampleContactDataCollector : IPersonalDataCollector
    {
        private readonly IActivityInfoProvider activityInfoProvider;
        private readonly ICountryInfoProvider countryInfoProvider;
        private readonly IStateInfoProvider stateInfoProvider;
        private readonly IConsentAgreementInfoProvider consentAgreementInfoProvider;
        private readonly IAccountContactInfoProvider accountContactInfoProvider;
        private readonly IAccountInfoProvider accountInfoProvider;
        private readonly IBizFormInfoProvider bizFormInfoProvider;


        /// <summary>
        /// Constructs a new instance of the <see cref="SampleContactDataCollector"/>.
        /// </summary>
        /// <param name="activityInfoProvider">Activity info provider.</param>
        /// <param name="countryInfoProvider">Country info provider.</param>
        /// <param name="stateInfoProvider">State info provider.</param>
        /// <param name="consentAgreementInfoProvider">Consent agreement info provider.</param>
        /// <param name="accountContactInfoProvider">Account contact info provider.</param>
        /// <param name="accountInfoProvider">Account info provider.</param>
        /// <param name="bizFormInfoProvider">BizForm info provider.</param>
        public SampleContactDataCollector(
            IActivityInfoProvider activityInfoProvider,
            ICountryInfoProvider countryInfoProvider,
            IStateInfoProvider stateInfoProvider,
            IConsentAgreementInfoProvider consentAgreementInfoProvider,
            IAccountContactInfoProvider accountContactInfoProvider,
            IAccountInfoProvider accountInfoProvider,
            IBizFormInfoProvider bizFormInfoProvider)
        {
            this.activityInfoProvider = activityInfoProvider;
            this.countryInfoProvider = countryInfoProvider;
            this.stateInfoProvider = stateInfoProvider;
            this.consentAgreementInfoProvider = consentAgreementInfoProvider;
            this.accountContactInfoProvider = accountContactInfoProvider;
            this.accountInfoProvider = accountInfoProvider;
            this.bizFormInfoProvider = bizFormInfoProvider;
        }


        /// <summary>
        /// Collects personal data based on given <paramref name="identities"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="outputFormat">Defines an output format for the result.</param>
        /// <returns><see cref="PersonalDataCollectorResult"/> containing personal data.</returns>
        public PersonalDataCollectorResult Collect(IEnumerable<BaseInfo> identities, string outputFormat)
        {
            using var writer = CreateWriter(outputFormat);
            var dataCollector = new SampleContactDataCollectorCore(writer, activityInfoProvider, countryInfoProvider, stateInfoProvider, consentAgreementInfoProvider,
                accountContactInfoProvider, accountInfoProvider, bizFormInfoProvider);
            return new PersonalDataCollectorResult
            {
                Text = dataCollector.CollectData(identities)
            };
        }


        private IPersonalDataWriter CreateWriter(string outputFormat) => outputFormat.ToLowerInvariant() switch
        {
            PersonalDataFormat.MACHINE_READABLE => new XmlPersonalDataWriter(),
            _ => new HumanReadablePersonalDataWriter(),
        };
    }
}
