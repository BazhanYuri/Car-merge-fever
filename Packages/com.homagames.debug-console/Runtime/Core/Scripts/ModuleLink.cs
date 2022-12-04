using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class ModuleLink : MonoBehaviour
    {
        public Toggle toggle;
        public TMP_Text title;

        public void Initialise(ToggleGroup group, UnityAction<bool> callback,HomaConsoleModule moduleName)
        {
            toggle.group = group;
            toggle.onValueChanged.AddListener(callback);
            callback.Invoke(toggle.isOn);
            title.text = moduleName.moduleName;
        }
    }
}