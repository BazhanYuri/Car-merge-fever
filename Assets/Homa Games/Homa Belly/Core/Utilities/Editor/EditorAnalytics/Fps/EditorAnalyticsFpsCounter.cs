#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED && UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Helper class listening playModeStateChanged and spawning
    /// an FPS Tracker
    /// </summary>
    public class EditorAnalyticsFpsCounter
    {
        private static GameObject _trackerGameObject;
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                CreateAndStartTracker();
            }
            else
            {
                GameObject.DestroyImmediate(_trackerGameObject);
            }
        }

        private static void CreateAndStartTracker()
        {
            _trackerGameObject = new GameObject("EditorAnalyticsFpsCounter");
            _trackerGameObject.AddComponent<Tracker>();
            _trackerGameObject.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(_trackerGameObject);
        }
        
        /// <summary>
        /// MonoBehaviour object to be spawned and measure FPS
        /// </summary>
        private class Tracker : MonoBehaviour
        {
            /// <summary>
            /// Time interval to track FPS in seconds
            /// </summary>
            private const int FPS_TRACK_S = 30;
            private const float FPS_MEASURE_PERIOD = 0.5f;
            private int _fpsAccumulator = 0;
            private float _fpsNextPeriod = 0;
            private int _currentFps;
            private float _timer = 0;
            
            private void Awake()
            {
                _fpsNextPeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
            }
            
            private void Update()
            {
                // measure average frames per second
                _fpsAccumulator++;
                if (Time.realtimeSinceStartup > _fpsNextPeriod)
                {
                    _currentFps = (int) (_fpsAccumulator/FPS_MEASURE_PERIOD);
                    _fpsAccumulator = 0;
                    _fpsNextPeriod += FPS_MEASURE_PERIOD;
                }
                
                _timer += Time.deltaTime;
                
                // Track if reached FPS_TRACK_S
                if (_timer > FPS_TRACK_S)
                {
                    _timer = 0;
                    EditorAnalytics.TrackEditorAnalyticsEvent("editor_runtime_fps", null, null, 0, _currentFps);
                }
            }
        }
    }
}
#endif
