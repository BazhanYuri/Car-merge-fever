using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class HomaConsole : MonoBehaviour
    {
        public const string Version = "v1.1.0";

        /// <summary>
        /// Triggered when the main Console window opened / closed.
        /// </summary>
        public static event System.Action<bool> OnConsoleVisibilityChanged;
        /// <summary>
        /// Will send true if anything from the console becomes visible and false when everything from the console is hidden.
        /// </summary>
        public static event System.Action<bool> OnModuleVisibilityChanged;

        [SerializeField]
        private TMP_Text versionFooter;
        [SerializeField]
        private RectTransform consoleRoot;
        [SerializeField]
        private ModuleLink moduleLinksPrefab;
        [SerializeField]
        private RectTransform moduleLinksRoot;
        [SerializeField]
        private TMP_Text moduleHeader;
        [SerializeField]
        private Toggle modulePinToggle;
        [SerializeField]
        private ToggleGroup moduleLinksToggleGroup;
        [SerializeField]
        private List<HomaConsoleModule> modules = new List<HomaConsoleModule>();


        private Coroutine _currentAnimation;
        private static bool _isOpen;

        private Dictionary<ModuleLink, HomaConsoleModule> _moduleLinkToModule =
            new Dictionary<ModuleLink, HomaConsoleModule>();

        private bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                consoleRoot.gameObject.SetActive(value);
                foreach (var module in modules)
                {
                    module.SetDisplayMode(value
                        ? HomaConsoleModule.DisplayMode.InConsole
                        : HomaConsoleModule.DisplayMode.InGame);
                }

                if (!value)
                    foreach (var module in modules)
                    {
                        module.IsVisible = module.KeepInGame;
                    }
                else
                {
                    EnsureCanvasIsOnTop();
                    foreach (var links in _moduleLinkToModule)
                    {
                        links.Value.IsVisible = links.Key.toggle.isOn;
                    }
                }
                if(!modules.Any(module => module.KeepInGame))
                    OnModuleVisibilityChanged?.Invoke(value);
                OnConsoleVisibilityChanged?.Invoke(value);
            }
        }

        private bool _isMenuOpen;

        private bool IsMenuOpen
        {
            get => _isMenuOpen;
            set
            {
                _isMenuOpen = value;
                if (_currentAnimation != null)
                    StopCoroutine(_currentAnimation);
                _currentAnimation = this.Animate((t) =>
                    {
                        consoleRoot.offsetMin = new Vector2(t, consoleRoot.offsetMin.y);
                        consoleRoot.offsetMax = new Vector2(t, consoleRoot.offsetMax.y);
                    },
                    Mathf.Lerp, 0.1f, consoleRoot.offsetMin.x, value ? 400 : 0);
            }
        }

        private HomaConsoleModule _currentModule;
        public HomaConsoleModule CurrentModule => _currentModule;

        private Canvas _canvas; 

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _canvas = GetComponent<Canvas>();
            EnsureCanvasIsOnTop();
            versionFooter.text = Version;
            modulePinToggle.onValueChanged.AddListener(OnModulePinToggle);
        }

        private void Start()
        {
            InitUI();
            IsOpen = false;
        }

        void InitUI()
        {
            foreach (var module in modules)
            {
                ModuleLink link = Instantiate(moduleLinksPrefab, moduleLinksRoot);
                link.Initialise(moduleLinksToggleGroup, (isOn) =>
                    {
                        module.IsVisible = isOn;
                        moduleHeader.text = module.moduleName;
                        _currentModule = module;
                        modulePinToggle.gameObject.SetActive(module.CanBeLocked);
                        modulePinToggle.isOn = module.KeepInGame;
                        module.SetDisplayMode(IsOpen
                            ? HomaConsoleModule.DisplayMode.InConsole
                            : HomaConsoleModule.DisplayMode.InGame);
                        if (isOn)
                            IsMenuOpen = false;
                    },
                    module);
                _moduleLinkToModule.Add(link, module);
            }
        }

        public void ToggleConsole()
        {
            IsOpen = !IsOpen;
        }

        public void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        private void OnModulePinToggle(bool isOn)
        {
            CurrentModule.KeepInGame = isOn;
        }

        private void EnsureCanvasIsOnTop()
        {
#if UNITY_2020_1_OR_NEWER
            var canvases = FindObjectsOfType<Canvas>(true);
#else
            var canvases = FindObjectsOfType<Canvas>();
#endif
            var childrenCanvases = GetComponentsInChildren<Canvas>(true);
            int maxSortOrder = 0;
            foreach (var canvas in canvases)
            {
                if (! childrenCanvases.Contains(canvas) 
                        && canvas.sortingOrder > maxSortOrder)
                    maxSortOrder = canvas.sortingOrder;
            }

            maxSortOrder = Mathf.Min(maxSortOrder, short.MaxValue - 1);
            if(_canvas.sortingOrder <= maxSortOrder)
                _canvas.sortingOrder = maxSortOrder + 1;
        }
    }
}
