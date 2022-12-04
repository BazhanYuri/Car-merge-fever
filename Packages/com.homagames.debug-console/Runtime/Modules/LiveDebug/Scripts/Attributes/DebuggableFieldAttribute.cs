using System;
using UnityEngine.Scripting;

namespace HomaGames.HomaConsole.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class DebuggableFieldAttribute : DebuggableAttribute
    {
        /// <summary>
        ///  Additional layout option to display fields or properties.
        /// </summary>
        public LayoutOption LayoutOption = LayoutOption.Default;
        /// <summary>
        /// A minimum value for the property.
        /// </summary>
        public float Min;
        /// <summary>
        /// A maximum value for the property.
        /// </summary>
        public float Max;
        public DebuggableFieldAttribute()
        {
            Paths = Array.Empty<string>();
        }
        
        public DebuggableFieldAttribute(params string[] paths)
        {
            Paths = paths;
        }
    }

    /// <summary>
    /// Additional layout option to display fields or properties.
    /// Joystick : will draw Vector2 as a Joystick
    /// Slider : will draw float as a slider
    /// ZoomSlider : will draw float as a delta based slider
    /// </summary>
    [Flags]
    public enum LayoutOption
    {
        Default,
        Joystick,
        Slider,
        ZoomSlider
    }
}