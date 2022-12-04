using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(Button))]
    public class ButtonLongPress : MonoBehaviour , IPointerDownHandler,IPointerUpHandler
    {
        private Button button;
        private float lastTimePressed;
        private bool clicking;
        public float longPressTime;
        public float sensitivity = 0.1f;
        [Serializable]
        public class FloatUnityEvent : UnityEvent<float>{}

        public FloatUnityEvent onLongPress;
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Update()
        {
            if (clicking && Time.realtimeSinceStartup - lastTimePressed > longPressTime)
            {
                onLongPress.Invoke((Time.realtimeSinceStartup - lastTimePressed - longPressTime)*sensitivity);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            clicking = true;
            lastTimePressed = Time.realtimeSinceStartup;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            clicking = false;
        }
    }
}