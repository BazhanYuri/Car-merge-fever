using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class ValueSlider : MonoBehaviour , IDragHandler,IPointerDownHandler,IPointerUpHandler
    {
        public float sensibility = 0.5f;
        [Serializable]
        public class FloatUnityEvent : UnityEvent<float>{}

        public FloatUnityEvent onDragged;

        private TMP_InputField inputField;
        private float pointedDownTime;
        
        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
            inputField.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x) && eventData.delta.magnitude > 10)
            {
                var parent = transform.parent;

                if (parent != null)
                {
                    eventData.pointerDrag = ExecuteEvents.GetEventHandler<IBeginDragHandler>(parent.gameObject);

                    if (eventData.pointerDrag != null)
                    {
                        ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                    }
                }

                return;
            }
            inputField.enabled = false;
            onDragged.Invoke(eventData.delta.x * sensibility);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            inputField.enabled = false;
            pointedDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - pointedDownTime < 0.5f && !eventData.dragging)
            {
                inputField.enabled = true;
            }
        }
    }
}