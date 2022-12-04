using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    public static class BootStrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            var prefab = Resources.Load<HomaConsole>("HomaConsole Canvas");
            Object.Instantiate(prefab.gameObject);
        }
    }

}