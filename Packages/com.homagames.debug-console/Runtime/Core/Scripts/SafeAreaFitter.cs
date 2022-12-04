using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    internal class SafeAreaFitter : MonoBehaviour
    {
        public CanvasScaler canvasScaler;
        private DeviceOrientation _deviceOrientation;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            RefreshLayout();
        }

        private void Start()
        {
            RefreshLayout();
        }

        private void Update()
        {
            var orientation = Input.deviceOrientation;
            if (orientation != _deviceOrientation)
            {
                if (_deviceOrientation != DeviceOrientation.Unknown)
                    RefreshLayout();
            }

            _deviceOrientation = orientation;
        }

        public void RefreshLayout()
        {
            var scaleFactor = canvasScaler.transform.localScale.x;
            _rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x, BottomMargin()) / scaleFactor;
            _rectTransform.offsetMax = new Vector2(_rectTransform.offsetMax.x, -TopMargin()) / scaleFactor;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        private float BottomMargin()
        {
            return Screen.safeArea.y;
        }
        
        private float TopMargin()
        {
            return Screen.height - Screen.safeArea.height - Screen.safeArea.y;
        }
    }
}