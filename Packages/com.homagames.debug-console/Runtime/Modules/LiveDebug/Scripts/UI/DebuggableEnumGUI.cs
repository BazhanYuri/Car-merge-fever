using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableEnumGUI : DebuggableFieldBaseGUI<Enum>
    {
        public TMP_Dropdown dropdown;

        private bool DataInitialized;
        private string[] EnumNames;
        private Enum[] EnumValues;

        private Enum _displayedValue;
        protected override Enum DisplayedValue
        {
            get => _displayedValue;
            set {
                if(value!=null) {
                    _displayedValue = value;
                    dropdown.value = Array.IndexOf(EnumValues, _displayedValue);
                }
            }
        }

        protected override void OnEnable()
        {
            InitializeData();
            
            base.OnEnable();
            dropdown.onValueChanged.AddListener(OnDropDownValueChange);
            // Fix dropdown being blank on window reopening 
            dropdown.RefreshShownValue();
        }

        private void OnDropDownValueChange(int arg0)
        {
            UpdateValue(EnumValues[arg0]);
        }

        protected void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(OnDropDownValueChange);
        }

        protected override void RefreshUI() {
            InitializeData();
            
            base.RefreshUI();
            if (Property.propertyType != null)
            {
                dropdown.options = new List<TMP_Dropdown.OptionData>();
                foreach (var enumOption in EnumNames)
                {
                    dropdown.options.Add(new TMP_Dropdown.OptionData()
                    {
                        text = enumOption
                    });
                }
            }
        }
        
        private void InitializeData() {
            if (DataInitialized || Property.propertyType == null)
                return;

            EnumNames = Enum.GetNames(DataType);
            
            Array enumValueArray = Enum.GetValues(DataType);
            EnumValues = new Enum[enumValueArray.Length];
            for (int i = 0; i < enumValueArray.Length; i++) {
                EnumValues[i] = (Enum)enumValueArray.GetValue(i);
            }
            
            // Fixed DisplayedValue being null
            DisplayedValue = EnumValues.First();

            DataInitialized = true;
        }
    }
}