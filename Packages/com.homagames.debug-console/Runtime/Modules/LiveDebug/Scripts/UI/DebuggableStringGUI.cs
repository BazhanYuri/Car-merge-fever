using TMPro;
using UnityEngine;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableStringGUI : DebuggableFieldBaseGUI<string>
    {
        public TMP_InputField text;

        protected override void OnEnable()
        {
            base.OnEnable();
            text.onValueChanged.AddListener(OnTextChanged);
        }

        private void OnTextChanged(string arg0)
        {
            UpdateValue(arg0);
        }

        protected override string DisplayedValue
        {
            get => text.text;
            set => text.text = value;
        }

        protected void OnDisable()
        {
            text.onValueChanged.RemoveListener(OnTextChanged);
        }
    }
}
