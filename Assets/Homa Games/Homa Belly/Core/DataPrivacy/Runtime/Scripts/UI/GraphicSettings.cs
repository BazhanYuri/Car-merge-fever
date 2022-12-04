using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public abstract class GraphicSettings<T> : MonoBehaviour where T : Component
    {
        private T _component;

        private void Awake()
        {
            _component = GetComponent<T>();
        }

        void Start()
        {
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            DataPrivacy.Settings settings = DataPrivacy.Settings.Load();
            if (settings != null)
            {
                UpdateGraphics(settings, _component);
            }
        }

        protected abstract void UpdateGraphics(DataPrivacy.Settings settings, T component);

#if UNITY_EDITOR

        private void OnValidate()
        {
            _component = GetComponent<T>();
            UpdateGraphics();
        }
#endif
    }
}