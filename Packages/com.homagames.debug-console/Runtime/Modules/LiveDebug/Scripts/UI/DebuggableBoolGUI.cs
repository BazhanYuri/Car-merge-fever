using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableBoolGUI : DebuggableFieldBaseGUI<bool>
    {
        public Toggle toggle;

        protected override void OnEnable()
        {
            base.OnEnable();
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool arg0)
        {
            UpdateValue(arg0);
        }

        protected void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        protected override bool DisplayedValue
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }
    }
}