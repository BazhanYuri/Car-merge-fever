using System;
using HomaGames.HomaConsole.Performance;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class PerformanceModule : HomaConsoleModule
    {
        public RectTransform inGameRoot;
        public RectTransform inConsoleRoot;
        public PerformanceManager graphyManager;

        private bool _isVisible;

        public override bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                gameObject.SetActive(value);
                graphyManager.gameObject.SetActive(value);
            }
        }

        public override void SetDisplayMode(DisplayMode displayMode)
        {
            var rootTransform = graphyManager.GetComponent<RectTransform>();
            switch (displayMode)
            {
                case DisplayMode.InGame:
                    rootTransform.SetParent(inGameRoot);
                    rootTransform.SetAsFirstSibling();
                    break;
                case DisplayMode.InConsole:
                    rootTransform.SetParent(inConsoleRoot);
                    break;
            }

            rootTransform.offsetMax = new Vector2();
            rootTransform.offsetMin = new Vector2();
        }

        public override bool CanBeLocked => true;
    }
}