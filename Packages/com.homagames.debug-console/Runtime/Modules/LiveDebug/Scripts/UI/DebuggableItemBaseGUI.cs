using System;
using System.Collections.Generic;
using System.IO;
using HomaGames.HomaConsole.Core.Attributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public abstract class DebuggableItemBaseGUI : MonoBehaviour
    {
        [SerializeField] protected TMP_Text label;
        [SerializeField] private DebugConsole.PropertyMeta _property;
        [SerializeField] private string fullPath;

        private Object _selectedObject;
        
        public DebugConsole.PropertyMeta Property
        {
            get => _property;
            set
            {
                _property = value;
                RefreshUI();
            }
        }

        public string FullPath
        {
            get =>  fullPath;
            set
            {
                fullPath = value;
                RefreshUI();
            }
        }

        protected CanvasGroup _canvasGroup;
        protected RawImage _backgroundImage;

        protected Object SelectedObject
        {
            get
            {
                Type t = DebugConsole.GetTargetType(FullPath);
                if (!_selectedObject && typeof(UnityEngine.Object).IsAssignableFrom(t))
                {
#if UNITY_2020_1_OR_NEWER
                    var found = FindObjectOfType(t,true);
#else
                    var found = FindObjectOfType(t);
#endif
                    if (found)
                        _selectedObject = found;
                }
                return _selectedObject;
            }
        }

        public abstract Type DataType { get; }

        public abstract LayoutOption LayoutOption { get; }

        protected virtual void OnEnable()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if(!_canvasGroup)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            if (!_backgroundImage)
            {
                var g = new GameObject("Background");
                var rt = g.AddComponent<RectTransform>();
                rt.SetParent(transform);
                rt.SetAsFirstSibling();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                _backgroundImage = g.AddComponent<RawImage>();
                _backgroundImage.raycastTarget = false;
                _backgroundImage.color = new Color(0, 0, 0, 0.1f);
            }
            RefreshUI();
        }

        protected virtual void RefreshUI()
        {
            if (!string.IsNullOrEmpty(_property.propertyName))
            {
                label.text = NicifyVariableName(Property.propertyName);
                _canvasGroup.blocksRaycasts = !Property.IsReadOnly;
                gameObject.SetActive(Property.isStatic || SelectedObject);
            }
        }

        private string NicifyVariableName(string name)
        {
            var result = "";
     
            for(int i = 0; i < name.Length; i++)
            {
                if(char.IsUpper(name[i]) && (i == 0 || !char.IsUpper(name[i -1])) && i != 0)
                {
                    result += " ";
                }
                result += name[i];
            }
            return result;
        }
    }
}