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
using UnityEngine.UI;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class ToggleExtension : MonoBehaviour
    {
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private GameObject checkmarkOffGameObject;

        private Color ToggleOffColor;
        private Color ToggleOnColor;

        private bool LastState = false;

        private async void Awake()
        {
            ToggleOffColor = new Color(0.85f, 0.85f, 0.85f);
            ToggleOnColor = new Color(0.28f, 0.84f, 0.67f);
            
            DataPrivacy.Settings settings = await DataPrivacy.Settings.LoadAsync();
            if (settings != null)
            {
                ToggleOnColor = settings.ToggleColor;
            }
            
            ToggleCheckmarkOff(LastState);
        }
        
        /// <summary>
        /// Toggles Checkmark Off game object active depending
        /// on toggle status. This checkmark is shown only when
        /// toggle is OFF
        /// </summary>
        /// <param name="isToggleOn"></param>
        public void ToggleCheckmarkOff(bool isToggleOn)
        {
            // Allows for ToggleCheckmarkOff calls before the resources are loaded
            LastState = isToggleOn;
            
            if (checkmarkOffGameObject != null)
            {
                checkmarkOffGameObject.SetActive(!isToggleOn);
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = isToggleOn ? ToggleOnColor : ToggleOffColor;
            }
        }
    }
}
