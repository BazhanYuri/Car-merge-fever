using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableVector2GUI : DebuggableFieldBaseGUI<Vector2>
    {
        public TMP_InputField inputFieldX;
        public TMP_InputField inputFieldY;

        private Vector2 displayed;
        protected override Vector2 DisplayedValue
        {
            get => displayed;
            set
            {
                displayed = value;
                UpdateDisplayed(displayed);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            inputFieldX.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputFieldY.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputFieldX.onValueChanged.AddListener(OnXTextFieldChange);
            inputFieldY.onValueChanged.AddListener(OnYTextFieldChange);
        }

        private void UpdateDisplayed(Vector3 newValue)
        {
            inputFieldX.text = newValue.x.ToString(CultureInfo.InvariantCulture);
            inputFieldY.text = newValue.y.ToString(CultureInfo.InvariantCulture);
        }

        private void OnXTextFieldChange(string arg0)
        {
            if (float.TryParse(arg0,NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
                displayed.x = res;
            UpdateValue(displayed);
        }
        
        private void OnYTextFieldChange(string arg0)
        {
            if (float.TryParse(arg0,NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
                displayed.y = res;
            UpdateValue(displayed);
        }

        public void OnXDragged(float delta)
        {
            displayed.x += delta;
            UpdateDisplayed(displayed);
        }
        
        public void OnYDragged(float delta)
        {
            displayed.y += delta;
            UpdateDisplayed(displayed);
        }

        protected void OnDisable()
        {
            inputFieldX.onValueChanged.RemoveListener(OnXTextFieldChange);
            inputFieldY.onValueChanged.RemoveListener(OnYTextFieldChange);
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