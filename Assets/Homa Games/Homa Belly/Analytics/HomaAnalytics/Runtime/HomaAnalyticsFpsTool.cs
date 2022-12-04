using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// You can start measuring FPS calling CreateFpsTool or just placing this component in a GameObject if you need more control over its settings. 
    /// </summary>
    public class HomaAnalyticsFpsTool : MonoBehaviour
    {
        private const string AVERAGE_FPS_EVENT_NAME = "AverageFPS";
        private const string CRITICAL_FPS_EVENT_NAME = "CriticalFPS";
        private const string APP_TARGET_FPS_EVENT_NAME = "AppTargetFPS";
        
        private static HomaAnalyticsFpsTool m_instance = null;
        
        [SerializeField,Tooltip("Use this to avoid measuring FPS while the app is initializing")]
        private float m_secondsBeforeStartMeasuring = 30f;
        
        [SerializeField,Tooltip("We will send a single event with the average fps after this time.")]
        private float m_secondsToSendAverageFps = 30f;
        
        [SerializeField,Tooltip("Amount of average FPS at which we will send a critical FPS event")]
        private float m_fpsCriticalThreshold = 15f;
        
        private readonly Dictionary<string, object> m_reusableEventValues = new Dictionary<string, object>(1);
        private float m_lastUpdateAvg;
        private int m_frameCountAvg;
        private int m_frameCountCritical;
        private float m_lastUpdateCritical;
        private int m_criticalFpsCount;
        private HomaAnalytics m_analytics = null;

        /// <summary>
        /// Call it during play mode. This will create a game object with the needed component to start measuring FPS.
        /// </summary>
        public static void CreateFpsTool(HomaAnalytics homaAnalytics,float delayBeforeFirstMeasure,float fpsCriticalThreshold)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[WARNING] You can't create the Fps tool if the application isn't playing");
                return;
            }
            
            if (m_instance != null)
            {
                m_instance.m_analytics = homaAnalytics;
                return;
            }
            
            var gameObject = new GameObject("HomaAnalyticsFpsTool", typeof(HomaAnalyticsFpsTool));
            if (gameObject.TryGetComponent<HomaAnalyticsFpsTool>(out var fpsTool))
            {
                fpsTool.m_analytics = homaAnalytics;
                fpsTool.m_secondsBeforeStartMeasuring = delayBeforeFirstMeasure;
                fpsTool.m_fpsCriticalThreshold = fpsCriticalThreshold;
            }
        }

        private void Awake()
        {
            // I want to avoid having more than one Fps tool in the scene by mistake
            if (m_instance != null && m_instance != this)
            {
                Destroy(this);
                return;
            }
            
            m_instance = this;
            DontDestroyOnLoad(this);
            
            StartCoroutine(SubmitFPSRoutine());
            StartCoroutine(CheckCriticalFPSRoutine());
        }

        private void OnDestroy()
        {
            if (m_instance != null && m_instance == this)
            {
                m_instance = null;
            }
        }

        private void Update()
        {
            m_frameCountAvg++;
            m_frameCountCritical++;
        }

        private IEnumerator SubmitFPSRoutine()
        {
            yield return new WaitForSeconds(m_secondsBeforeStartMeasuring);
            m_frameCountAvg = 0;
            
            while(Application.isPlaying)
            {
                yield return new WaitForSeconds(m_secondsToSendAverageFps);
                SubmitFpsEvents();
            }
        }
        
        private IEnumerator CheckCriticalFPSRoutine()
        {
            yield return new WaitForSeconds(m_secondsBeforeStartMeasuring);
            m_frameCountCritical = 0;
            
            while(Application.isPlaying)
            {
                yield return new WaitForSeconds(1f);
                CheckCriticalFPS();
            }
        }

        private void SubmitFpsEvents()
        {
            float timeSinceUpdate = Time.unscaledTime - m_lastUpdateAvg;

            // Average FPS
            if (timeSinceUpdate > 1.0f)
            {
                float fpsSinceUpdate = m_frameCountAvg / timeSinceUpdate;
                m_lastUpdateAvg = Time.unscaledTime;
                m_frameCountAvg = 0;

                if (fpsSinceUpdate > 0)
                {
                    if (m_analytics != null)
                    {
                        TrackEvent(AVERAGE_FPS_EVENT_NAME, "AverageFps",Mathf.RoundToInt(fpsSinceUpdate),true);
                    }
                }
            }
            
            // Critical FPS
            if (m_criticalFpsCount > 0)
            {
                TrackEvent(CRITICAL_FPS_EVENT_NAME,"CriticalFpsCount",m_criticalFpsCount,false);
                m_criticalFpsCount = 0;
            }
        }
        
        private void CheckCriticalFPS()
        {
            float timeSinceUpdate = Time.unscaledTime - m_lastUpdateCritical;

            if (timeSinceUpdate >= 1.0f)
            {
                float fpsSinceUpdate = m_frameCountCritical / timeSinceUpdate;
                m_lastUpdateCritical = Time.unscaledTime;
                m_frameCountCritical = 0;

                if (fpsSinceUpdate <= m_fpsCriticalThreshold)
                {
                    m_criticalFpsCount++;
                }
            }
        }

        private void TrackEvent(string eventName,string fpsValueKey, int fps,bool trackTargetFps)
        {
            string fpsString = fps.ToString(CultureInfo.InvariantCulture);
            
            m_reusableEventValues.Clear();
            m_reusableEventValues.Add(fpsValueKey,fpsString);
            
            if (trackTargetFps)
            {
                m_reusableEventValues.Add(APP_TARGET_FPS_EVENT_NAME,
                    Application.targetFrameRate.ToString(CultureInfo.InvariantCulture));
            }
            
            var fpsEvent = new RuntimeAnalyticsEvent(eventName, 
                HomaAnalytics.PROFILE_CATEGORY, 
                m_reusableEventValues);

            m_analytics?.TrackEvent(fpsEvent);
        }
    }
}