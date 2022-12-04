using System;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.DataPrivacy;
using UnityEngine;
using UnityEngine.UI;

public class DataPrivacySettings : MonoBehaviour
{
    [SerializeField] private Toggle SettingsAnalyticsAcceptanceToggle;
    [SerializeField] private Toggle SettingsTailoredAdsAcceptanceToggle;

    public static event Action OnDismiss;

    private void Start()
    {
        SettingsAnalyticsAcceptanceToggle.onValueChanged.AddListener(OnSettingsAnalyticsAcceptanceChanged);
        SettingsTailoredAdsAcceptanceToggle.onValueChanged.AddListener(OnTailoredAdsAcceptanceChanged);
        SettingsAnalyticsAcceptanceToggle.isOn = Manager.Instance.IsAnalyticsGranted();
        SettingsTailoredAdsAcceptanceToggle.isOn = Manager.Instance.IsTailoredAdsGranted();
    }

#if UNITY_ANDROID
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Dismiss();
        }
    }
#endif

    private void OnDisable()
    {
        SettingsAnalyticsAcceptanceToggle.onValueChanged.RemoveListener(OnSettingsAnalyticsAcceptanceChanged);
        SettingsTailoredAdsAcceptanceToggle.onValueChanged.RemoveListener(OnTailoredAdsAcceptanceChanged);
    }

    private void OnTailoredAdsAcceptanceChanged(bool arg0)
    {
        PlayerPrefs.SetInt(Constants.PersistenceKey.TAILORED_ADS, arg0 ? 1 : 0);
    }

    private void OnSettingsAnalyticsAcceptanceChanged(bool arg0)
    {
        PlayerPrefs.SetInt(Constants.PersistenceKey.ANALYTICS_TRACKING, arg0 ? 1 : 0);
    }

    public void Dismiss()
    {
        PlayerPrefs.Save();
        HomaBelly.Instance.SetAnalyticsTrackingConsentGranted(Manager.Instance.IsAnalyticsGranted());
        HomaBelly.Instance.SetTailoredAdsConsentGranted(Manager.Instance.IsTailoredAdsGranted());

        OnDismiss?.Invoke();
        Destroy(gameObject);
    }
}