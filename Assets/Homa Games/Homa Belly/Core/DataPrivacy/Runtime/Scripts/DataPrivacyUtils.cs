using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public static class DataPrivacyUtils
    {
        public static void LoadNextScene()
        {
            int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            while (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings 
                   && MustSkipScene(SceneUtility.GetScenePathByBuildIndex(nextSceneBuildIndex)))
                nextSceneBuildIndex++;
                
            if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneBuildIndex);
            }
            else
            { 
                Debug.LogError("[Homa Games DataPrivacy] There is no next scene available in Build Settings");
            }
        }

        public static void LoadNextGameScene()
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex + 1;

            while (buildIndex < SceneManager.sceneCountInBuildSettings && IsSceneDataPrivacyScene(buildIndex))
            {
                buildIndex++;
            }

            if (buildIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(buildIndex);
            }
            else
            {
                Debug.LogWarning("[Homa Games DataPrivacy] There is no game scene available in Build Settings");
            }
        }
        
        public static bool IsSceneDataPrivacyScene(int sceneBuildIndex)
        {
            return SceneUtility.GetScenePathByBuildIndex(sceneBuildIndex).Contains("DataPrivacy");
        }
        
        private static bool MustSkipScene(string sceneName)
        {
            if (sceneName.Contains("IDFA"))
                return !Manager.Instance.IsiOS14_5OrHigher;

            return false;
        }
    }
}