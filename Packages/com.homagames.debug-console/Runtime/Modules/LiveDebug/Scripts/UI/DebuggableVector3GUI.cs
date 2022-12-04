using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableVector3GUI : DebuggableFieldBaseGUI<Vector3>
    {
        public TMP_InputField inputFieldX;
        public TMP_InputField inputFieldY;
        public TMP_InputField inputFieldZ;

        private Vector3 displayed;
        protected override Vector3 DisplayedValue
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
            inputFieldZ.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputFieldX.onValueChanged.AddListener(OnXTextFieldChange);
            inputFieldY.onValueChanged.AddListener(OnYTextFieldChange);
            inputFieldZ.onValueChanged.AddListener(OnZTextFieldChange);
        }

        private void UpdateDisplayed(Vector3 newValue)
        {
            inputFieldX.text = newValue.x.ToString(CultureInfo.InvariantCulture);
            inputFieldY.text = newValue.y.ToString(CultureInfo.InvariantCulture);
            inputFieldZ.text = newValue.z.ToString(CultureInfo.InvariantCulture);
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
        
        private void OnZTextFieldChange(string arg0)
        {
            if (float.TryParse(arg0,NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
                displayed.z = res;
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
        
        public void OnZDragged(float delta)
        {
            displayed.z += delta;
            UpdateDisplayed(displayed);
        }

        protected void OnDisable()
        {
            inputFieldX.onValueChanged.RemoveListener(OnXTextFieldChange);
            inputFieldY.onValueChanged.RemoveListener(OnYTextFieldChange);
            inputFieldZ.onValueChanged.RemoveListener(OnZTextFieldChange);
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