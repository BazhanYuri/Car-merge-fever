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

using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class Constants
    {
        // Assets
        public const string DATA_PRIVACY_SETTINGS_ASSET_PATH = "Assets/Homa Games/Homa Belly/Preserved/DataPrivacy/Resources/Homa Games DataPrivacy Settings.asset";
        public const string DATA_PRIVACY_PREFAB_ASSET_PATH = "Assets/Homa Games/Homa Belly/Core/DataPrivacy/Runtime/Resources/Homa Games DataPrivacy.prefab";
        public const string SETTINGS_RESOURCE_NAME = "Homa Games DataPrivacy Settings";
        public const string PREFAB_RESOURCE_NAME = "Homa Games DataPrivacy";

        // Colors
        public const string FONT_COLOR = "#4A4F58";
        public const string SECONDARY_FONT_COLOR = "#31D2A4";
        public const string BUTTON_FONT_COLOR = "#FFFFFF";
        public const string BACKGROUND_COLOR = "#FFFFFF";
        public const string TOGGLE_COLOR = "#47D7AC";

        public class PersistenceKey
        {
            public const string ABOVE_REQUIRED_AGE = "homagames.gdpr.above_required_age";
            public const string TERMS_AND_CONDITIONS = "homagames.gdpr.terms_and_conditions";
            public const string ANALYTICS_TRACKING = "homagames.gdpr.analytics_tracking";
            public const string TAILORED_ADS = "homagames.gdpr.tailored_ads";
            public const string IOS_ADS_TRACKING_ASKED = "homagames.idfa.ios_ads_tracking_asked";
            public const string IOS_GLOBAL_ADS_TRACKING_SETTING = "homagames.idfa.ios_global_ads_tracking_setting";
            public const string IS_GDPR_PROTECTED = "homagames.is_GDPR_protected";
            public const string HAS_DATAPRIVACY_FLOW_BEEN_COMPLETED = "homagames.dataprivacy.flow_completed";
        }
    }   
}
