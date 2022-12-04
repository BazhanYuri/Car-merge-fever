using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public static class DataPrivacyFlowNotifier
    {
        private static bool? _flowCompleted;
        
        public static bool FlowCompleted
        {
            get {
                if (! _flowCompleted.HasValue)
                    _flowCompleted = PlayerPrefs.GetInt(Constants.PersistenceKey.HAS_DATAPRIVACY_FLOW_BEEN_COMPLETED, 0) == 1
                        // Backward compatibility: old GDPR used IsTermsAndConditionsAccepted as "flow completed"
                        || Manager.Instance.IsTermsAndConditionsAccepted();

                return _flowCompleted.Value;
            }
            private set
            {
                _flowCompleted = value;
                PlayerPrefs.SetInt(Constants.PersistenceKey.HAS_DATAPRIVACY_FLOW_BEEN_COMPLETED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private static event Action _onFlowCompleted;
        public static event Action OnFlowCompleted
        {
            add
            {
                if (FlowCompleted)
                    value.Invoke();
                else
                    _onFlowCompleted += value;
            }
            remove
            {
                if (! FlowCompleted)
                    _onFlowCompleted -= value;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializationFlow()
        {
            if (FlowCompleted)
            {
                return;
            }
#if UNITY_EDITOR
            if(DataPrivacyUtils.IsSceneDataPrivacyScene(SceneManager.GetActiveScene().buildIndex))            
#endif
            WaitForGdprValuesSet(() =>
            {
                WaitForIdfaValueSet(SetFlowCompleted);
            });
#if UNITY_EDITOR
            else
                SetFlowCompleted();
#endif
        }

        private static void WaitForGdprValuesSet(Action after)
        {
            GdprView.OnGdprValuesSet += after;
        }

        private static void WaitForIdfaValueSet(Action after)
        {
#if UNITY_IOS
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                var settingsTask = DataPrivacy.Settings.LoadAsync();

                settingsTask.ContinueWithOnMainThread(task =>
                {
                    if (task.Result.ShowIdfa)
                    {
                        IdfaView.OnIdfaValuesSet += after.Invoke;
                    }
                    else
                    {
                        after.Invoke();
                    }
                });
            }
            else
            {
                after.Invoke();
            }
#else
        after.Invoke();
#endif
        }

        private static void SetFlowCompleted()
        {
            FlowCompleted = true;
            _onFlowCompleted?.Invoke();
        }
    }
}