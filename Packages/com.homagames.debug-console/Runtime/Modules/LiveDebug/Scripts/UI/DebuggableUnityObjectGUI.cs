using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HomaGames.HomaConsole.Layout
{
    public class DebuggableUnityObjectGUI : DebuggableFieldBaseGUI<Object>
    {
        public TMP_Dropdown dropdown;

        // Object to index in the enum list
        private Dictionary<Object, int> _possibleValues = new Dictionary<Object, int>();
        private Dictionary<int, Object> _possibleValuesInverted = new Dictionary<int, Object>();
        
        void AddValue(Object unityObject)
        {
            if (unityObject == null || _possibleValues.ContainsKey(unityObject))
                return;
            dropdown.options.Add(new TMP_Dropdown.OptionData()
            {
                text = $"[{unityObject.GetType().Name}] {unityObject.name}"
            });
            var dropdownIndex = dropdown.options.Count - 1;
            _possibleValues.Add(unityObject,dropdownIndex);
            _possibleValuesInverted.Add(dropdownIndex,unityObject);
        }
        
        protected override Object DisplayedValue
        {
            get => _possibleValuesInverted.TryGetValue(dropdown.value, out Object unityObject) ? unityObject : null;
            set
            {
                if(value != null && _possibleValues.TryGetValue(value, out int index))
                {
                    dropdown.value = index;
                }
                else
                {
                    AddValue(value);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            dropdown.onValueChanged.AddListener(OnDropDownValueChange);
            // Fix dropdown being blank on window reopening 
            dropdown.RefreshShownValue();
        }

        private void OnDropDownValueChange(int arg0)
        {
            UpdateValue(_possibleValuesInverted[arg0]);
        }

        protected void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(OnDropDownValueChange);
        }

        protected override void RefreshUI()
        {
            base.RefreshUI();
            if (Property.propertyType != null)
            {
                List<Object> foundObjects = new List<Object>(FindObjectsOfType(Property.propertyType));
                foundObjects.AddRange(Resources.FindObjectsOfTypeAll(Property.propertyType));
                foreach (var unityObject in foundObjects)
                {
                    AddValue(unityObject);
                }
            }
        }
    }
}