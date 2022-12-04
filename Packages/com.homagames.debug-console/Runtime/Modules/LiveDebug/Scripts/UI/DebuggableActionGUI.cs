using System;
using HomaGames.HomaConsole.Core.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public sealed class DebuggableActionGUI : DebuggableItemBaseGUI
    {
        [SerializeField] private Button button;

        public override Type DataType => typeof(System.Action);
        public override LayoutOption LayoutOption => LayoutOption.Default;

        protected override void OnEnable()
        {
            base.OnEnable();
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            DebugConsole.Invoke(FullPath, SelectedObject);
        }
    }
}