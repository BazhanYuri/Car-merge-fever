using System;
using HomaGames.HomaConsole.Core.Attributes;
using TMPro;
using UnityEngine;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public abstract class DebuggableFieldBaseGUI<T> : DebuggableItemBaseGUI
    {
        public override Type DataType => Property.propertyType ?? typeof(T);
        
        public override LayoutOption LayoutOption => LayoutOption.Default;

        protected abstract T DisplayedValue { get; set; }

        private void Awake()
        {
            DisplayedValue = default;
        }

        protected void UpdateValue(T value)
        {
            DebugConsole.Set(FullPath, value, SelectedObject);
        }

        private void LateUpdate()
        {
            if (!String.IsNullOrEmpty(Property.propertyName))
            {
                var newValue = DebugConsole.Get<T>(FullPath, SelectedObject);
                if ((DisplayedValue==null && newValue!=null) || 
                    (DisplayedValue!=null && !DisplayedValue.Equals(newValue) && newValue != null))
                {
                    DisplayedValue = newValue;
                }
            }
        }
    }
}