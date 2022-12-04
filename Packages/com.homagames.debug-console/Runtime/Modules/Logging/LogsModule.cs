using System;
using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaConsole.Logs;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class LogsModule : HomaConsoleModule
    {
        public RectTransform inConsoleRoot;
        public RectTransform inGameRoot;
        public DebugLogManager debugLogManager;
        public Button closeButton;
        public RectTransform resizeButton;

        private bool _isVisible;

        public override bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                gameObject.SetActive(value);
            }
        }

        public override void SetDisplayMode(DisplayMode displayMode)
        {
            var rootTransform = debugLogManager.GetComponent<RectTransform>();
            var windowTransform = debugLogManager.transform.GetChild(0).GetComponent<RectTransform>();
            switch (displayMode)
            {
                case DisplayMode.InGame:
                    closeButton.gameObject.SetActive(true);
                    resizeButton.gameObject.SetActive(true);
                    rootTransform.SetParent(inGameRoot);
                    rootTransform.SetAsLastSibling();
                    debugLogManager.PopupEnabled = true;
                    debugLogManager.HideLogWindow();
                    windowTransform.anchorMin = new Vector2(windowTransform.anchorMin.x, 0.8f);
                    break;
                case DisplayMode.InConsole:
                    closeButton.gameObject.SetActive(false);
                    resizeButton.gameObject.SetActive(false);
                    rootTransform.SetParent(inConsoleRoot);
                    debugLogManager.PopupEnabled = false;
                    debugLogManager.ShowLogWindow();
                    windowTransform.anchorMin = new Vector2(windowTransform.anchorMin.x, 0);
                    break;
            }

            rootTransform.offsetMax = new Vector2();
            rootTransform.offsetMin = new Vector2();
        }

        public override bool CanBeLocked => true;
    }
}