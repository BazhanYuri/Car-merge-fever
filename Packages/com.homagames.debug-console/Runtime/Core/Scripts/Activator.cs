using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class Activator : MonoBehaviour
    {
        public enum AnchorMode
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        
        public Button button;
        public UnityEvent OnActivated;
        private RectTransform _rectTransform;
        private AnchorMode _anchorMode = AnchorMode.TopLeft;

        public AnchorMode Mode
        {
            get => _anchorMode;
            set
            {
                _anchorMode = value;
                switch (_anchorMode)
                {
                    case AnchorMode.TopLeft:
                        _rectTransform.anchorMax = new Vector2(0,1);
                        _rectTransform.anchorMin = new Vector2(0,1);
                        _rectTransform.pivot = new Vector2(0, 1);
                        break;
                    case AnchorMode.TopRight:
                        _rectTransform.anchorMax = new Vector2(1,1);
                        _rectTransform.anchorMin = new Vector2(1,1);
                        _rectTransform.pivot = new Vector2(1, 1);                        
                        break;
                    case AnchorMode.BottomLeft:
                        _rectTransform.anchorMax = new Vector2(0,0);
                        _rectTransform.anchorMin = new Vector2(0,0);
                        _rectTransform.pivot = new Vector2(0, 0);                        
                        break;
                    case AnchorMode.BottomRight:
                        _rectTransform.anchorMax = new Vector2(1,0);
                        _rectTransform.anchorMin = new Vector2(1,0);
                        _rectTransform.pivot = new Vector2(1, 0);                        
                        break;
                }
            }
        }
        private float _size = 100;

        public float Size
        {
            get => _size;
            set
            {
                _size = value;
                _rectTransform.sizeDelta = Vector2.one * _size;
            }
        }
        private Queue<float> _clicks = new Queue<float>();

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            var settings = HomaConsoleSettings.GetOrCreateSettings();
            if (settings)
            {
                Mode = settings.activatorMode;
                Size = settings.activatorSize;
            }
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _clicks.Enqueue(Time.time);
            while (_clicks.Count > 0 && Time.time - _clicks.Peek() > 1)
                _clicks.Dequeue();
            if (_clicks.Count > 2)
            {
                OnActivated.Invoke();
                _clicks.Clear();
            }
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClicked);
        }
    }
}