using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableFloatGUI : DebuggableFieldBaseGUI<float>
    {
        public TMP_InputField inputField;

        protected override float DisplayedValue
        {
            get => float.TryParse(inputField.text,NumberStyles.Float, CultureInfo.InvariantCulture, out float res) ? res : default;
            set => inputField.text = value.ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputField.onValueChanged.AddListener(OnTextFieldChange);
        }

        private void OnTextFieldChange(string arg0)
        {
            if (float.TryParse(arg0,NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
                UpdateValue(res);
        }

        public void OnDragged(float delta)
        {
            DisplayedValue += delta;
        }
        
        public void Increment(float delta)
        {
            DisplayedValue += delta;
        }

        protected void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnTextFieldChange);
        }

        protected override void RefreshUI()
        {
            base.RefreshUI();
            foreach (var button in GetComponentsInChildren<Button>())
            {
                button.gameObject.SetActive(!Property.IsReadOnly);
            }
        }
    }
}