using System;
using RealManager = HomaGames.HomaBelly.DataPrivacy.Manager;

namespace HomaGames.GDPR
{
    [Obsolete("HomaGames.GDPR.Manager has been moved to HomaGames.HomaBelly.DataPrivacy.Manager.")]
    public sealed class Manager
    {

        #region Singleton pattern

        private static readonly Manager instance = new Manager();

        public static Manager Instance
        {
            get { return instance; }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Callback invoked when the DataPrivacy UI is shown
        /// </summary>
        public static event System.Action OnShow
        {
            add => RealManager.OnShow += value;
            remove => RealManager.OnShow -= value;
        }

        /// <summary>
        /// Callback invoked when the DataPrivacy UI is dismissed. When this
        /// method gets invoked, all user decisions can be retrieved
        /// through corresponding Manager accessors.
        /// </summary>
        public static event Action OnDismiss
        {
            add => RealManager.OnDismiss += value;
            remove => RealManager.OnDismiss -= value;
        }

        public static bool IsGdprProtectedRegion => RealManager.IsGdprProtectedRegion;

        public bool IsiOS14_5OrHigher => RealManager.Instance.IsiOS14_5OrHigher;
        #endregion

        #region Public methods

        /// <summary>
        /// Show the DataPrivacy UI
        /// </summary>
        public void Show()
        {
#pragma warning disable CS4014
            RealManager.Instance.Show();
#pragma warning restore CS4014
        }

        /// <summary>
        /// Show the DataPrivacy UI
        /// </summary>
        public async void Show(bool internetReachable, bool forceDisableDataPrivacy)
        {
            await RealManager.Instance.Show(internetReachable, forceDisableDataPrivacy);
        }

        /// <summary>
        /// Obtain either if user is above required age or not.
        /// </summary>
        /// <returns>True if user explicitly asserted being above the required age. False otherwise</returns>
        public bool IsAboveRequiredAge()
        {
            return RealManager.Instance.IsAboveRequiredAge();
        }

        /// <summary>
        /// Obtain either if user accepted Terms & Conditions or not.
        /// </summary>
        /// <returns>True if user accepted Terms & Conditions. False otherwise</returns>
        public bool IsTermsAndConditionsAccepted()
        {
            return RealManager.Instance.IsTermsAndConditionsAccepted();
        }

        /// <summary>
        /// Obtain either if user granted Analytics tracking or not.
        /// </summary>
        /// <returns>True if user granted Analytics tracking. False otherwise</returns>
        public bool IsAnalyticsGranted()
        {
            return RealManager.Instance.IsAnalyticsGranted();
        }

        /// <summary>
        /// Obtain either if user granted Tailored Ads permission or not.
        /// </summary>
        /// <returns>True if user granted Tailored Ads permission. False otherwise</returns>
        public bool IsTailoredAdsGranted()
        {
            return RealManager.Instance.IsTailoredAdsGranted();
        }

        /// <summary>
        /// Return if the IOS IDFA onboarding flow has been asked already.
        /// </summary>
        /// <returns>True if already asked.</returns>
        public bool IsIOSIDFAFlowDone()
        {
            return RealManager.Instance.IsIOSIDFAFlowDone();
        }

        #endregion
    }
}