using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// This component helps developer design UIs with banners and Unity's safe area.
    ///
    /// WHen placed on a Canvas GameObject, it will take its child named "SafeArea", and resize
    /// it to fit the inside of <see cref="Screen.safeArea">Screen.safeArea</see>, while also taking
    /// in account the banners displayed on screen. 
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class BannerSafeAreaHelper : MonoBehaviour
    {
        public BannerPosition DefaultBannerPosition;

        [Tooltip("Check this if you use multiple banner placement IDs.")]
        public bool EnableExtraPlacementId;

        public List<ExtraPlacement> ExtraPlacements;

        private static readonly List<BannerSafeAreaHelper> InstanceList = new List<BannerSafeAreaHelper>();
        
        private RectTransform SafeAreaTransform;
        private Canvas Canvas;
        private Rect CurrentRect;

        private void OnEnable()
        {
            InstanceList.Add(this);
        }

        private void OnDisable()
        {
            InstanceList.Remove(this);
        }

        private void Awake()
        {
            
            SafeAreaTransform = transform.Find("SafeArea") as RectTransform;
            Canvas = GetComponent<Canvas>();
        }

        void Start()
        {
            CurrentRect = ComputeRect();
            UpdateRect(CurrentRect);
        }

        void Update()
        {
            if (InstanceList.FirstOrDefault() == this)
            {
                BannerSafeAreaHelperUpdater.Update();
            }
        }

        private void ComputeAndUpdateRect()
        {
            Rect newRect = ComputeRect();

            if (newRect == CurrentRect)
                return;

            CurrentRect = newRect;
            UpdateRect(CurrentRect);
        }
        
        private Rect ComputeRect()
        {
            Rect outputRect = Screen.safeArea;

            ApplyBannerPosition(ref outputRect, null, DefaultBannerPosition);

            if (EnableExtraPlacementId)
            {
                foreach (var extraPlacement in ExtraPlacements)
                {
                    if (!string.IsNullOrWhiteSpace(extraPlacement.PlacementId))
                        ApplyBannerPosition(ref outputRect, extraPlacement.PlacementId, extraPlacement.BannerPosition);
                }
            }

            return outputRect;
        }

        private void ApplyBannerPosition(ref Rect outputRect, string bannerPlacementId, BannerPosition bannerPosition)
        {
            float bannerSize = GetBannerHeight(bannerPlacementId);

            switch (bannerPosition)
            {
                case BannerPosition.BOTTOM:
                {
                    float yDifference = bannerSize - outputRect.y;
                    if (yDifference > 0)
                    {
                        outputRect.y += yDifference;
                        outputRect.height -= yDifference;
                    }

                    break;
                }
                case BannerPosition.TOP:
                {
                    float yDifference = bannerSize - (Screen.height - outputRect.yMax);
                    if (yDifference > 0)
                    {
                        outputRect.height -= yDifference;
                    }

                    break;
                }
            }
        }
        
        private void UpdateRect(Rect rect)
        {
            if (! SafeAreaTransform)
                return;
            
            var anchorMin = rect.position;
            var anchorMax = rect.position + rect.size;
            
            var canvasPixelRect = Canvas.pixelRect;
            anchorMin.x /= canvasPixelRect.width;
            anchorMin.y /= canvasPixelRect.height;
            anchorMax.x /= canvasPixelRect.width;
            anchorMax.y /= canvasPixelRect.height;

            SafeAreaTransform.anchorMin = anchorMin;
            SafeAreaTransform.anchorMax = anchorMax;
        }

        [Serializable]
        public class ExtraPlacement
        {
            public string PlacementId;
            public BannerPosition BannerPosition;
        }
        
        #region BannerSizeCache

        private static readonly Dictionary<string, int> BannerSizeCache = new Dictionary<string, int>();

        private static int GetBannerHeight(string placementId)
        {
            if (BannerSizeCache.TryGetValue(placementId, out var size))
                return size;

            size = HomaBelly.Instance.GetBannerHeight(placementId);
            BannerSizeCache[placementId] = size;
            return size;
        }

        #endregion

        /// <summary>
        /// The goal of this class is to reduce the computation cost of BannerSafeAreaHelpers.
        ///
        /// Because the SafeArea/Banner layout is rarely updated, and Getting the banner size is really
        /// expensive because of native calls, this class will update <see cref="HelpersToUpdatePerFrame"/>
        /// Helper per frame, and clear <see cref="BannerSizeCache"/> once all helpers are updated.
        /// </summary>
        private static class BannerSafeAreaHelperUpdater
        {
            private const int HelpersToUpdatePerFrame = 3;

            private static int CurrentIndex;
            private static float Cooldown;
            
            public static void Update()
            {
                
                
                for (int i = 0; i < HelpersToUpdatePerFrame; i++)
                {
                    if (CurrentIndex >= InstanceList.Count)
                    {
                        CurrentIndex = 0;
                        BannerSizeCache.Clear();
                    }

                    InstanceList[CurrentIndex].ComputeAndUpdateRect();

                    CurrentIndex++;
                }
            }
        }
    }
}
