using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class SystemInfoModule : HomaConsoleModule
    {
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
        }

        public override bool CanBeLocked => false;
    }
}