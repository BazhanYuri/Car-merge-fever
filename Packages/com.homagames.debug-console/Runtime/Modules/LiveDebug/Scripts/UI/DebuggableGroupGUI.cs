using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableGroupGUI : MonoBehaviour
    {
        [SerializeField] private string fullPath;
        [SerializeField] protected TMP_Text label;
        private RectTransform _rectTransform;

        private RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        private List<RectTransform> elements = new List<RectTransform>();

        public string FullPath
        {
            get => fullPath;
            set
            {
                fullPath = value;
                UseToggle = GroupName == "";
                toggle.isOn = Toggled;
                RefreshUI();
            }
        }

        public string GroupName => PathUtility.GetGroupNameFromFullPath(fullPath);

        private bool UseToggle
        {
            set
            {
                toggle.gameObject.SetActive(!value);
                GetComponent<FlowLayoutGroup>().padding.top = value ? 0 : 60;
            }
        }

        private bool Toggled
        {
            get => PlayerPrefs.GetInt($"homa_console_{fullPath}_toggled") == 1;
            set
            {
                foreach (var rectTransform in elements)
                {
                    rectTransform.gameObject.SetActive(value);
                }
                PlayerPrefs.SetInt($"homa_console_{fullPath}_toggled", value ? 1 : 0);
            }
        }

        public Toggle toggle;

        public void AddElement(RectTransform rectTransform)
        {
            rectTransform.SetParent(RectTransform);
            elements.Add(rectTransform);
            if(GroupName != "")
                rectTransform.gameObject.SetActive(Toggled);
        }

        protected void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnToggleChange);
        }

        private void OnToggleChange(bool arg0)
        {
            Toggled = arg0;
        }

        private void RefreshUI()
        {
            label.text = GroupName;
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleChange);
        }
    }
}