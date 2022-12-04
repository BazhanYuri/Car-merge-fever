using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HomaGames.HomaConsole.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Layout
{
    [AddComponentMenu("")]
    public class DebugPanelWindow : MonoBehaviour
    {
        public List<DebuggableItemBaseGUI> debuggableItemBaseGUI = new List<DebuggableItemBaseGUI>();
        public Transform contentRoot;
        public DebuggableGroupGUI debuggableGroupGUI;

        // Paths to ui component
        private Dictionary<string, DebuggableGroupGUI> _instancedGroupGUIs =
            new Dictionary<string, DebuggableGroupGUI>();

        private void Awake()
        {
            ReflectionCore.LoadDebuggableData();
            DebugConsole.OnDebugItemChanged += RefreshUI;
        }

        private void Start()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (!contentRoot)
                return;
            for (int i = 0; i < contentRoot.childCount; i++)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }

            foreach (var path in DebugConsole.DebugPaths.OrderBy(kv => kv.Value.order))
            {
                var debugUI = InstantiateUI(path.Key, path.Value);
                if (debugUI)
                    GetOrCreateGroupForPath(PathUtility.GetParentPath(path.Key)).AddElement(debugUI.GetComponent<RectTransform>());
                else
                    Debug.LogWarning(
                        $"Unsupported debug item type {path.Value.propertyType} for field \"{path.Value.propertyName}\". Skipping.");
            }
        }

        private DebuggableItemBaseGUI InstantiateUI(string fullPath, DebugConsole.PropertyMeta property)
        {
            if (GetUIPrefabFromDataType(property, out GameObject prefab))
            {
                var ui = Instantiate(prefab, contentRoot, true);
                ui.transform.localScale = Vector3.one;
                DebuggableItemBaseGUI uiItem = ui.GetComponent<DebuggableItemBaseGUI>();
                uiItem.Property = property;
                uiItem.FullPath = fullPath;
                return uiItem;
            }

            return null;
        }

        private bool GetUIPrefabFromDataType(DebugConsole.PropertyMeta property, out GameObject prefab)
        {
            prefab = null;
            foreach (var uiPrefabs in debuggableItemBaseGUI)
            {
                if (uiPrefabs.DataType.IsAssignableFrom(property.propertyType)
                    && uiPrefabs.LayoutOption == property.layoutOption)
                {
                    prefab = uiPrefabs.gameObject;
                    return true;
                }
            }

            return false;
        }

        private DebuggableGroupGUI GetOrCreateGroupForPath(string fullPath)
        {
            if (_instancedGroupGUIs.TryGetValue(fullPath,out DebuggableGroupGUI group))
                return group;

            foreach (var groupPath in PathUtility.GetGroupPaths(fullPath))
            {
                if (_instancedGroupGUIs.ContainsKey(groupPath))
                    continue;
                string parentPath = PathUtility.GetParentPath(groupPath);
                var root = _instancedGroupGUIs.TryGetValue(parentPath,
                    out DebuggableGroupGUI parentGroupGUI) && parentPath != ""
                    ? parentGroupGUI.transform
                    : contentRoot;
                var ui = Instantiate(debuggableGroupGUI.gameObject,
                    root, true);
                ui.transform.localScale = Vector3.one;
                var groupComponent = ui.GetComponent<DebuggableGroupGUI>();
                _instancedGroupGUIs.Add(groupPath, groupComponent);
                if (parentPath != "" && parentGroupGUI != null)
                {
                    parentGroupGUI.AddElement(groupComponent.GetComponent<RectTransform>());
                    var layout = ui.AddComponent<LayoutElement>();
                    layout.preferredWidth = 800;
                }

                groupComponent.FullPath = groupPath;
            }

            return _instancedGroupGUIs[fullPath];
        }
    }
}