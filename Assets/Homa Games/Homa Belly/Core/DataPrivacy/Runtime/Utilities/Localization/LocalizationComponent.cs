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

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// Localization component to automatically retrieve the corresponding
    /// string value depending on the configured language
    /// </summary>
    public class LocalizationComponent : MonoBehaviour
    {
        public string LocalizationKey;
        [Tooltip("String value to be displayed as Localization fallback")]
        public string DefaultValue;
        private Text textComponent;

        private void Awake()
        {
            textComponent = GetComponent<Text>();
        }

        protected void Start()
        {
            UpdateLocalizationValue();
        }

        protected void OnEnable()
        {
            Localization.OnLocalizationLanguageLoaded += OnLocalizationLanguageLoaded;
        }

        protected void OnDisable()
        {
            Localization.OnLocalizationLanguageLoaded -= OnLocalizationLanguageLoaded;
        }

        protected void OnLocalizationLanguageLoaded()
        {
            UpdateLocalizationValue();
        }

        protected void UpdateLocalizationValue()
        {
            if (textComponent != null)
            {
                textComponent.text = Localization.GetLocalizedValueForKey(LocalizationKey, DefaultValue);
            }
        }

        public void SetLocalizationKey(string key)
        {
            LocalizationKey = key;
            UpdateLocalizationValue();
        }

#if UNITY_EDITOR

        protected void OnValidate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Localization.OnLocalizationLanguageLoaded += OnLocalizationLanguageLoaded;
            
                Awake();
                UpdateLocalizationValue();
            }
        }
#endif
    }   
}
