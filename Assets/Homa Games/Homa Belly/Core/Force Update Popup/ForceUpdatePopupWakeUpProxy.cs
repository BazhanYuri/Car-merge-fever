using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class ForceUpdatePopupWakeUpProxy : MonoBehaviour
    {
        private static bool ProxySetup;
        
        public static void SetupProxy()
        {
            if (ProxySetup)
                return;
            
            GameObject go = new GameObject(nameof(ForceUpdatePopupWakeUpProxy));
            DontDestroyOnLoad(go);
            go.AddComponent<ForceUpdatePopupWakeUpProxy>();
            ProxySetup = true;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus == false)
            {
                ForceUpdatePopup.ShowPopupIfRequired();
            }
        }
    }
}