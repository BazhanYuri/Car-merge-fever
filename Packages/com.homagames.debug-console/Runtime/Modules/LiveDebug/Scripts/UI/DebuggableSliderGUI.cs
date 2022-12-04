using System;
using System.Globalization;
using HomaGames.HomaConsole.Core.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public abstract class DebuggableSliderGUI<T> : DebuggableFieldBaseGUI<T>
    {
        public Slider slider;
        public TMP_Text min, max;
        public TMP_Text valueText;
        public override LayoutOption LayoutOption => LayoutOption.Slider;

        protected override void OnEnable()
        {
            slider.onValueChanged.AddListener(OnSliderMove);
            base.OnEnable();
        }

        protected override void RefreshUI()
        {            
            base.RefreshUI();
            
            if (Property.min < Property.max)
            {
                slider.minValue = Property.min;
                slider.maxValue = Property.max;
                min.text = Property.min.ToString(CultureInfo.InvariantCulture);
                max.text = Property.max.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                min.text = slider.minValue.ToString(CultureInfo.InvariantCulture);
                max.text = slider.maxValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected abstract void OnSliderMove(float arg0);

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnSliderMove);
        }
    }
}