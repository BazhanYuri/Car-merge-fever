using System;
using HomaGames.HomaConsole.UI.HSVPicker;
using UnityEngine;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableColorGUI : DebuggableFieldBaseGUI<Color>
    {
        public ColorPicker colorPicker;

        protected override Color DisplayedValue
        {
            get => colorPicker.CurrentColor;
            set => colorPicker.CurrentColor = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            colorPicker.onValueChanged.AddListener(OnColorChange);
        }

        private void OnColorChange(Color arg0)
        {
            UpdateValue(arg0);
        }

        private void OnDisable()
        {
            colorPicker.onValueChanged.RemoveListener(OnColorChange);
        }
    }
}