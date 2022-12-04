using System;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class IdfaView : MonoBehaviour
    {
        [SerializeField] private GameObject progressBarFillThree;
        [SerializeField] private GameObject prepopupAfterGdpr;
        [SerializeField] private GameObject prepopupAfterGdprCallToActionButton;
        [SerializeField] private GameObject prepopupWithoutGdpr;
        [SerializeField] private GameObject prepopupWithoutGdprCallToActionButton;
        [SerializeField] private GameObject authorizationBackground;

        #region OnIdfaValuesSet definition 
        private static bool _idfaValuesSet;
        private static bool IdfaValuesSet
        {
            get => _idfaValuesSet;
            set
            {
                bool fromFalseToTrue = _idfaValuesSet != value && value;
                _idfaValuesSet = value;
                
                if (fromFalseToTrue)
                    _onIdfaValuesSet?.Invoke();
            }
        }
        public static event Action _onIdfaValuesSet;
        public static event Action OnIdfaValuesSet
        {
            add
            {
                if (IdfaValuesSet)
                    value.Invoke();
                
                _onIdfaValuesSet += value;
            }
            remove => _onIdfaValuesSet -= value;
        }
        #endregion

        private DataPrivacy.Settings Settings;

        private async void Awake()
        {
            var loadRequest = DataPrivacy.Settings.LoadAsync();

            InitializationHelper.TrackDesignEvent("prepopup_open");

            Settings = await loadRequest;
            InitializeUIStatus();
            
            if (! Settings.ShowIdfaPrePopup)
                RequestAuthorization();
        }

        public void RequestAuthorization()
        {
            UpdateUIForAuthorizationPopup();

#if !UNITY_EDITOR
            if (Settings.ShowIdfa)
            {
                AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
                AppTrackingTransparency.RequestTrackingAuthorization();
            }
#else
            OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus.AUTHORIZED);
#endif
            InitializationHelper.TrackDesignEvent("native_idfa_popup_request");
            InitializationHelper.TrackAttributionEvent("native_idfa_popup_request");
        }
        
        private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus obj)
        {
            authorizationBackground.SetActive(false);
            if (obj != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
            {
                InitializationHelper.TrackDesignEvent("native_idfa_popup_tracking_not_allowed");
                InitializationHelper.TrackAttributionEvent("native_idfa_popup_tracking_not_allowed");
            }
            else
            {
                InitializationHelper.TrackDesignEvent("native_idfa_popup_tracking_allowed");
                InitializationHelper.TrackAttributionEvent("native_idfa_popup_tracking_allowed");
            }

            Dismiss();
        }

        private void Dismiss()
        {
            AppTrackingTransparency.OnAuthorizationRequestDone -= OnAuthorizationRequestDone;
            PlayerPrefs.SetInt(Constants.PersistenceKey.IOS_ADS_TRACKING_ASKED, 1);
            PlayerPrefs.Save();
            authorizationBackground.SetActive(false);

            IdfaValuesSet = true;
            DataPrivacyUtils.LoadNextScene();
        }

        private void UpdateUIForAuthorizationPopup()
        {
            progressBarFillThree.SetActive(true);
            authorizationBackground.SetActive(true);
            prepopupAfterGdprCallToActionButton.SetActive(false);
            prepopupWithoutGdprCallToActionButton.SetActive(false);
        }

        private void InitializeUIStatus()
        {
            progressBarFillThree.SetActive(false);
            authorizationBackground.SetActive(false);
            prepopupAfterGdprCallToActionButton.SetActive(true);
            prepopupWithoutGdprCallToActionButton.SetActive(true);
            
            if (Settings.ShowIdfaPrePopup)
            {
                if (Manager.IsGdprProtectedRegion && Settings.GdprEnabled &&
                    !Settings.ForceDisableGdpr)
                {
                    // Show default design DataPrivacy continuation explanation prepopup
                    prepopupAfterGdpr.SetActive(true);
                    prepopupWithoutGdpr.SetActive(false);
                }
                else
                {
                    // Show detailed explanation prepopup, as GDPR was not shown before
                    prepopupAfterGdpr.SetActive(false);
                    prepopupWithoutGdpr.SetActive(true);
                }
            }
            else
            {
                prepopupAfterGdpr.SetActive(false);
                prepopupWithoutGdpr.SetActive(false);
            }
        }
    }
}