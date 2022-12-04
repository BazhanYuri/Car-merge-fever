using System;
using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaConsole;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class LiveDebugModule : HomaConsoleModule
    {
        private static LiveDebugModule instance;
        public static LiveDebugModule Instance => instance;
        
        private bool _isVisible;
        public RectTransform inGameRoot;
        public RectTransform inConsoleRoot;

        private RectTransform _rectTransform;
        
        public override bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                gameObject.SetActive(value);
            }
        }

        private void Awake()
        {
            instance = this;
            _rectTransform = GetComponent<RectTransform>();
        }

        public override void SetDisplayMode(DisplayMode displayMode)
        {
            switch (displayMode)
            {
                case DisplayMode.InGame:
                    _rectTransform.SetParent(inGameRoot);
                    break;
                case DisplayMode.InConsole:
                    _rectTransform.SetParent(inConsoleRoot);
                    break;
            }
            _rectTransform.offsetMax = new Vector2();
            _rectTransform.offsetMin = new Vector2();
        }

        public override bool CanBeLocked => true;
    }
}