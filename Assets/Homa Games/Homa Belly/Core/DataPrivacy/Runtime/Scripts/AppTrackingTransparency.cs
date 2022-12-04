using UnityEngine;
using System;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// Use this class to show the iOS 14 App Tracking Transparency native
    /// popup requesting user's authorization to obtain Identifier for Advertising  (IDFA)
    /// </summary>
    ///
    /// <example>
    /// <code>
    ///     #if UNITY_IOS
    ///         AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
    ///         AppTrackingTransparency.RequestTrackingAuthorization();
    ///     #endif
    /// </code>
    /// </example>
    public class AppTrackingTransparency
    {
#if UNITY_IOS
        private delegate void AppTrackingTransparencyCallback(int result);
        [AOT.MonoPInvokeCallback(typeof(AppTrackingTransparencyCallback))]
        private static void appTrackingTransparencyCallbackReceived(int result)
        {
            Debug.Log($"UnityAppTrackingTransparencyCallback received: {result}");

            ThreadUtils.RunAndForgetOnMainThread(() =>
            {
                if (OnAuthorizationRequestDone != null)
                {
                    switch (result)
                    {
                        case 0:
                            OnAuthorizationRequestDone(AuthorizationStatus.NOT_DETERMINED);
                            break;
                        case 1:
                            OnAuthorizationRequestDone(AuthorizationStatus.RESTRICTED);
                            break;
                        case 2:
                            OnAuthorizationRequestDone(AuthorizationStatus.DENIED);
                            break;
                        case 3:
                            OnAuthorizationRequestDone(AuthorizationStatus.AUTHORIZED);
                            break;
                        default:
                            OnAuthorizationRequestDone(AuthorizationStatus.NOT_DETERMINED);
                            break;
                    }
                }
            });
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void requestTrackingAuthorization(AppTrackingTransparencyCallback callback);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void openPrivacySettings(AppTrackingTransparencyCallback callback);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string identifierForAdvertising();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int trackingAuthorizationStatus();

#endif

        /// <summary>
        /// Possible App Tracking Transparency authorization status 
        /// </summary>
        public enum AuthorizationStatus
        {
            NOT_DETERMINED,
            /// <summary>
            /// User restrited app tracking. IDFA not available
            /// </summary>
            RESTRICTED,
            /// <summary>
            /// User did not grant access to IDFA
            /// </summary>
            DENIED,
            /// <summary>
            /// You can safely request IDFA
            /// </summary>
            AUTHORIZED
        };

        /// <summary>
        /// Callback invoked once user made a decision through iOS App Tracking Transparency native popup
        /// </summary>
        public static Action<AuthorizationStatus> OnAuthorizationRequestDone;

        /// <summary>
        /// Obtain current Tracking Authorization Status
        /// </summary>
        public static AuthorizationStatus TrackingAuthorizationStatus
        {
            get
            {
#if UNITY_EDITOR
                Debug.Log("Running on Editor platform. Callback invoked with debug result");
                return AuthorizationStatus.AUTHORIZED;
#elif UNITY_IOS
                return (AuthorizationStatus) trackingAuthorizationStatus();
#else
                return AuthorizationStatus.NOT_DETERMINED; 
#endif
            }
        }

        /// <summary>
        /// Requests iOS Tracking Authorization. <br/>
        /// 
        /// <see cref="OnAuthorizationRequestDone" /> will
        /// be invoked with the authorization result for every iOS version.
        ///
        /// <list type="bullet">
        ///     <item>
        ///         <b>iOS 13 and lower</b>: Obtains if Advertising Tracking is enabled or not
        ///     </item>
        ///     <item>
        ///         <b>iOS 14+</b>: The native iOS Tracking Authorization popup will be shown to the user
        ///     </item>
        /// </list>
        /// </summary>
        ///
        /// <example>
        /// <code>
        ///     #if UNITY_IOS
        ///         AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
        ///         AppTrackingTransparency.RequestTrackingAuthorization();
        ///     #endif
        /// </code>
        /// </example>
        public static void RequestTrackingAuthorization()
        {
#if UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
            OnAuthorizationRequestDone?.Invoke(AuthorizationStatus.AUTHORIZED);
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log("Requesting authorization to iOS...");
                requestTrackingAuthorization(appTrackingTransparencyCallbackReceived);
            }
            else
            {
            Debug.Log($"Platform '{Application.platform}' not supported");
            }
#else
            Debug.Log($"Platform '{Application.platform}' not supported");
#endif
        }

        /// <summary>
        /// Opens iOS App Settings screen
        /// </summary>
        ///
        /// <example>
        /// <code>
        ///     #if UNITY_IOS
        ///         AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
        ///         AppTrackingTransparency.OpenPrivacySettings();
        ///     #endif
        /// </code>
        /// </example>
        public static void OpenPrivacySettings()
        {
#if UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
            OnAuthorizationRequestDone?.Invoke(AuthorizationStatus.AUTHORIZED);
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log("Opening iOS settings...");
                openPrivacySettings(appTrackingTransparencyCallbackReceived);
            }
            else
            {
                Debug.Log($"Platform '{Application.platform}' not supported");
            }
#else
            Debug.Log($"Platform '{Application.platform}' not supported");
#endif
        }

        /// <summary>
        /// Obtains iOS Identifier for Advertising (IDFA) if authorized.
        /// </summary>
        /// <returns>The IDFA value if authorized, null otherwise</returns>
        public static string IdentifierForAdvertising()
        {
#if UNITY_EDITOR
            Debug.Log("Running on Editor platform. Callback invoked with debug result");
            return "unity-editor-test-idfa";
#elif UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log("Obtaining IDFA from iOS...");
                string idfa = identifierForAdvertising();
                return string.IsNullOrEmpty(idfa) ? null : idfa;
            }
            else
            {
                Debug.Log($"Platform '{Application.platform}' not supported");
                return null;
            }
#else
            Debug.Log($"Platform '{Application.platform}' not supported");
            return null;
#endif
        }
    }
}