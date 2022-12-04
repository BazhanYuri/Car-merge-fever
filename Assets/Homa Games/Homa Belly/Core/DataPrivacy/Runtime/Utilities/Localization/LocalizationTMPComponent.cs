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

#if homagames_gdpr_textmeshproavailable
using TMPro;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// Localization component to automatically retrieve the corresponding
    /// string value depending on the configured language
    /// </summary>
    public class LocalizationTMPComponent : LocalizationComponent
    {
#if homagames_gdpr_textmeshproavailable
        private TMP_Text tmpTextComponent;

        private void Awake()
        {
            tmpTextComponent = GetComponent<TMP_Text>();
        }
        
        protected new void Start()
        {
            UpdateLocalizationValue();
        }

        protected new void OnEnable()
        {
            Localization.OnLocalizationLanguageLoaded += OnLocalizationLanguageLoaded;
        }

        protected new void OnDisable()
        {
            Localization.OnLocalizationLanguageLoaded -= OnLocalizationLanguageLoaded;
        }

        protected new void OnLocalizationLanguageLoaded()
        {
            UpdateLocalizationValue();
        }

        protected new void UpdateLocalizationValue()
        {
            if (tmpTextComponent != null)
            {
                tmpTextComponent.text = Localization.GetLocalizedValueForKey(LocalizationKey, DefaultValue);
            }
        }

#if UNITY_EDITOR

        protected new void OnValidate()
        {
            Localization.OnLocalizationLanguageLoaded += OnLocalizationLanguageLoaded;

            Awake();
            UpdateLocalizationValue();
        }
#endif
#endif
    }
}
