using System;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [AddComponentMenu("")]
    public class HomaBellyComponentProxy : MonoBehaviour
    {
        private Action<bool> OnApplicationPauseAction;

        public void SetOnApplicationPause(Action<bool> action) => OnApplicationPauseAction = action;

        private void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPauseAction?.Invoke(pauseStatus);
        }
    }
}