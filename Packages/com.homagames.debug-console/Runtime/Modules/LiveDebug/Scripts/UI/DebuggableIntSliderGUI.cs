using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableIntSliderGUI : DebuggableSliderGUI<int>
    {
        protected override int DisplayedValue
        {
            get => Mathf.RoundToInt(slider.value);
            set
            {
                valueText.text = value.ToString(CultureInfo.InvariantCulture);
                slider.value = value;
            }
        }

        protected override void OnSliderMove(float arg0)
        {
            int intValue = Mathf.RoundToInt(arg0);
            valueText.text = intValue.ToString(CultureInfo.InvariantCulture);
            UpdateValue(intValue);
        }
    }
}