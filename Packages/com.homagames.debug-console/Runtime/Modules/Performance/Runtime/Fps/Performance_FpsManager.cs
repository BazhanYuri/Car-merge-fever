/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@tayx94)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            03-Jan-18
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using UnityEngine;
using System.Collections.Generic;
using HomaGames.HomaConsole.Performance.Utils;
using HomaGames.HomaConsole.Performance.UI;
using UnityEngine.UI;

namespace HomaGames.HomaConsole.Performance.Fps
{
    [AddComponentMenu("")]
    public class Performance_FpsManager : MonoBehaviour, IMovable, IModifiableState
    {
        #region Variables -> Serialized Private

        [SerializeField] private    GameObject                  m_fpsGraphGameObject = null;

        [SerializeField] private    List<GameObject>            m_nonBasicTextGameObjects   = new List<GameObject>();

        [SerializeField] private    List<Image>                 m_backgroundImages          = new List<Image>();

        #endregion

        #region Variables -> Private

        private                     PerformanceManager               m_graphyManager = null;
        
        private                     Performance_FpsGraph                  m_fpsGraph = null;
        private                     Performance_FpsMonitor                m_fpsMonitor = null;
        private                     Performance_FpsText                   m_fpsText = null;

        private                     RectTransform               m_rectTransform = null;

        private                     List<GameObject>            m_childrenGameObjects       = new List<GameObject>();

        private                     PerformanceManager.ModuleState   m_previousModuleState = PerformanceManager.ModuleState.FULL;
        private                     PerformanceManager.ModuleState   m_currentModuleState = PerformanceManager.ModuleState.FULL;
        
        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            UpdateParameters();
        }

        #endregion

        #region Methods -> Public

        public void SetPosition(PerformanceManager.ModulePosition newModulePosition)
        {
            float xSideOffset = Mathf.Abs(m_rectTransform.anchoredPosition.x);
            float ySideOffset = Mathf.Abs(m_rectTransform.anchoredPosition.y);

            switch (newModulePosition)
            {
                case PerformanceManager.ModulePosition.TOP_LEFT:

                    m_rectTransform.anchorMax           = Vector2.up;
                    m_rectTransform.anchorMin           = Vector2.up;
                    m_rectTransform.anchoredPosition    = new Vector2(xSideOffset, -ySideOffset);

                    break;

                case PerformanceManager.ModulePosition.TOP_RIGHT:

                    m_rectTransform.anchorMax           = Vector2.one;
                    m_rectTransform.anchorMin           = Vector2.one;
                    m_rectTransform.anchoredPosition    = new Vector2(-xSideOffset, -ySideOffset);

                    break;

                case PerformanceManager.ModulePosition.BOTTOM_LEFT:

                    m_rectTransform.anchorMax           = Vector2.zero;
                    m_rectTransform.anchorMin           = Vector2.zero;
                    m_rectTransform.anchoredPosition    = new Vector2(xSideOffset, ySideOffset);

                    break;

                case PerformanceManager.ModulePosition.BOTTOM_RIGHT:

                    m_rectTransform.anchorMax           = Vector2.right;
                    m_rectTransform.anchorMin           = Vector2.right;
                    m_rectTransform.anchoredPosition    = new Vector2(-xSideOffset, ySideOffset);

                    break;

                case PerformanceManager.ModulePosition.FREE:
                    break;
            }
        }

        public void SetState(PerformanceManager.ModuleState state, bool silentUpdate = false)
        {
            if (!silentUpdate)
            {
                m_previousModuleState = m_currentModuleState;
            }

            m_currentModuleState    = state;

            switch (state)
            {
                case PerformanceManager.ModuleState.FULL:
                    gameObject.SetActive(true);
                    m_childrenGameObjects.SetAllActive(true);
                    SetGraphActive(true);

                    if (m_graphyManager.Background)
                    {
                        m_backgroundImages.SetOneActive(0);
                    }
                    else
                    {
                        m_backgroundImages.SetAllActive(false);
                    }
                    
                    break;

                case PerformanceManager.ModuleState.TEXT:
                    gameObject.SetActive(true);
                    m_childrenGameObjects.SetAllActive(true);
                    SetGraphActive(false);
                    
                    if (m_graphyManager.Background)
                    {
                        m_backgroundImages.SetOneActive(1);
                    }
                    else
                    {
                        m_backgroundImages.SetAllActive(false);
                    }
                    
                    break;

                case PerformanceManager.ModuleState.BASIC:
                    gameObject.SetActive(true);
                    m_childrenGameObjects.SetAllActive(true);
                    m_nonBasicTextGameObjects.SetAllActive(false);
                    SetGraphActive(false);
                    
                    if (m_graphyManager.Background)
                    {
                        m_backgroundImages.SetOneActive(2);
                    }
                    else
                    {
                        m_backgroundImages.SetAllActive(false);
                    }

                    break;

                case PerformanceManager.ModuleState.BACKGROUND:
                    gameObject.SetActive(true);
                    m_childrenGameObjects.SetAllActive(false);
                    SetGraphActive(false);
                    
                    m_backgroundImages.SetAllActive(false);
                    break;

                case PerformanceManager.ModuleState.OFF:
                    gameObject.SetActive(false);
                    break;
            }
        }

        public void RestorePreviousState()
        {
            SetState(m_previousModuleState);
        }
        
        public void UpdateParameters()
        {
            foreach (var image in m_backgroundImages)
            {
                image.color = m_graphyManager.BackgroundColor;
            }
            
            m_fpsGraph      .UpdateParameters();
            m_fpsMonitor    .UpdateParameters();
            m_fpsText       .UpdateParameters();
            
            SetState(m_graphyManager.FpsModuleState);
        }

        public void RefreshParameters()
        {
            foreach (var image in m_backgroundImages)
            {
                image.color = m_graphyManager.BackgroundColor;
            }

            m_fpsGraph      .UpdateParameters();
            m_fpsMonitor    .UpdateParameters();
            m_fpsText       .UpdateParameters();

            SetState(m_currentModuleState, true);
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            m_graphyManager = transform.root.GetComponentInChildren<PerformanceManager>();
            
            m_rectTransform = GetComponent<RectTransform>();

            m_fpsGraph      = GetComponent<Performance_FpsGraph>();
            m_fpsMonitor    = GetComponent<Performance_FpsMonitor>();
            m_fpsText       = GetComponent<Performance_FpsText>();

            foreach (Transform child in transform)
            {
                if (child.parent == transform)
                {
                    m_childrenGameObjects.Add(child.gameObject);
                }
            }
        }

        private void SetGraphActive(bool active)
        {
            m_fpsGraph.enabled = active;
            m_fpsGraphGameObject.SetActive(active);
        }

        #endregion
    }
}