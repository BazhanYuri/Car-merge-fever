using UnityEngine;
using System;
using UnityEngine.Events;

namespace HomaGames.HomaConsole.UI.HSVPicker
{
    [Serializable]
    public class ColorChangedEvent : UnityEvent<Color>
    {

    }
}