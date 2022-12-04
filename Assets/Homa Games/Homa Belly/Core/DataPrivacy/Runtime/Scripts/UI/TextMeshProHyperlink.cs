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
using UnityEngine.EventSystems;

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// Clickable hyperlink behaviour for TextMeshPro
    /// </summary>
#if homagames_gdpr_textmeshproavailable
    [RequireComponent(typeof(TextMeshProUGUI))]
#endif
    public class TextMeshProHyperlink : MonoBehaviour
#if homagames_gdpr_textmeshproavailable
        , IPointerClickHandler
#endif
    {
#if homagames_gdpr_textmeshproavailable
        private TMP_Text textMeshProText;

        private void Awake()
        {
            textMeshProText = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshProText, Input.mousePosition, eventData.pressEventCamera);
            if (linkIndex != -1)
            { // was a link clicked?
                TMP_LinkInfo linkInfo = textMeshProText.textInfo.linkInfo[linkIndex];

                // open the link id as a url, which is the metadata we added in the text field
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }
#endif
    }
}
