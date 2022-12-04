using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(Toggle))]
    public class Foldout : MonoBehaviour
    {
        private Toggle _toggle;
        public RectTransform foldoutImage;
        
        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            OnToggle(_toggle.isOn);
            _toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool arg0)
        {
            if(arg0)
                foldoutImage.rotation = Quaternion.Euler(0,0,0);
            else
                foldoutImage.rotation = Quaternion.Euler(0,0,-90);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggle);
        }
    }
}