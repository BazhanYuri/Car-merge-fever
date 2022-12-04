using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableFloatSliderGUI : DebuggableSliderGUI<float>
    {
        protected override float DisplayedValue
        {
            get => slider.value;
            set
            {
                valueText.text = value.ToString(CultureInfo.InvariantCulture);
                slider.value = value;
            }
        }

        protected override void OnSliderMove(float arg0)
        {
            valueText.text = arg0.ToString(CultureInfo.InvariantCulture);
            UpdateValue(arg0);
        }
    }
}