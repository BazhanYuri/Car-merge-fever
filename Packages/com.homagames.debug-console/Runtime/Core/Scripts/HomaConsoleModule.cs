using System;
using UnityEngine;
using UnityEngine.Events;

namespace HomaGames.HomaConsole
{
    public abstract class HomaConsoleModule : MonoBehaviour
    {
        public enum DisplayMode
        {
            InGame,
            InConsole
        }

        public string moduleName;
        public bool KeepInGame { get; set; }

        public abstract bool IsVisible { get; set; }

        public abstract void SetDisplayMode(DisplayMode displayMode);
        
        public abstract bool CanBeLocked { get; }
    }
}