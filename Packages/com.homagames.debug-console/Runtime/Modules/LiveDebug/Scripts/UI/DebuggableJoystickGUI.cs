using HomaGames.HomaConsole.Core;
using HomaGames.HomaConsole.Core.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebuggableJoystickGUI : DebuggableFieldBaseGUI<Vector2>, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public RectTransform Background;
        public RectTransform Knob;
        public override LayoutOption LayoutOption => LayoutOption.Joystick;

        protected override Vector2 DisplayedValue
        {
            get => PointPosition;
            set => PointPosition = value;
        }

        private float offset = 1;
        Vector2 PointPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            PointPosition = new Vector2(
                        (eventData.position.x - Background.position.x) /
                        ((Background.rect.size.x - Knob.rect.size.x) / 2),
                        (eventData.position.y - Background.position.y) /
                        ((Background.rect.size.y - Knob.rect.size.y) / 2));

            PointPosition = (PointPosition.magnitude > 1.0f) ? PointPosition.normalized : PointPosition;

            Knob.transform.position = new Vector2(
                (PointPosition.x * ((Background.rect.size.x - Knob.rect.size.x) / 2) * offset) + Background.position.x,
                (PointPosition.y * ((Background.rect.size.y - Knob.rect.size.y) / 2) * offset) + Background.position.y);
            UpdateValue(PointPosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            PointPosition = new Vector2(0f, 0f);
            Knob.transform.position = Background.position;
            UpdateValue(PointPosition);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnEndDrag(eventData);
        }
    }
}