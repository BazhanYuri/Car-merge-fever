using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableIntGUI : DebuggableFieldBaseGUI<int>
    {
        public TMP_InputField inputField;

        protected override int DisplayedValue
        {
            get => int.TryParse(inputField.text,NumberStyles.Integer, CultureInfo.InvariantCulture, out int res) ? res : default;
            set => inputField.text = value.ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.onValueChanged.AddListener(OnTextFieldChange);
        }

        private void OnTextFieldChange(string arg0)
        {
            if (int.TryParse(arg0,NumberStyles.Integer, CultureInfo.InvariantCulture, out int res))
                UpdateValue(res);
        }

        public void OnDragged(float delta)
        {
            DisplayedValue += (int)delta;
        }
        
        public void Increment(float delta)
        {
            DisplayedValue += (int)delta;
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