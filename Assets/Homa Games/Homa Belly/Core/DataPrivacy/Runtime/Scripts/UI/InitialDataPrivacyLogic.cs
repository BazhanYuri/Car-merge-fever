/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// Script for default behavior showing the DataPrivacy screens within "Homa Games DataPrivacy Scene".
    /// Once user accepts to play, the next scene in Build Settings gets loaded.
    /// </summary>
    public class InitialDataPrivacyLogic : MonoBehaviour
    {
        private void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
#if UNITY_IOS
            // Track authorization initial status for iOS 14.5+
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                switch (AppTrackingTransparency.TrackingAuthorizationStatus)
                {
                    case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_not_determined");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_not_determined");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_allowed");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_allowed");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.DENIED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_denied");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_denied");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_restricted");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_restricted");
                        break;
                }
            }
#endif
        }

        private void Start()
        {
            // If TOS and PP are already accepted, skip DataPrivacy
            if (DataPrivacyFlowNotifier.FlowCompleted)
            {
                DataPrivacyUtils.LoadNextGameScene();
            }
            else
            {
                DataPrivacyUtils.LoadNextScene();
            }

        }
    }
}