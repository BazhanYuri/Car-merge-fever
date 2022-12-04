using System;
using HomaGames.HomaConsole.Core;
using HomaGames.HomaConsole.Core.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableZoomSliderGUI : DebuggableFieldBaseGUI<float> , IEndDragHandler,IPointerUpHandler
    {
        public Slider slider;
        public override LayoutOption LayoutOption => LayoutOption.ZoomSlider;

        protected override float DisplayedValue
        {
            get => slider.value;
            set => slider.value = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            slider.onValueChanged.AddListener(OnSliderMove);
        }

        private void OnSliderMove(float arg0)
        {
            UpdateValue(arg0);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnSliderMove);
        }

        private void Reset()
        {
            DisplayedValue = 0;
            UpdateValue(0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Reset();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Reset();
        }
    }
}