using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace HomaGames.HomaBelly
{
    public class CrossPromotionPlayer : MonoBehaviour
    {
        /// <summary>
        /// This image is rendering the cross promotion
        /// </summary>
        [SerializeField]
        private RawImage m_displayImage;
        /// <summary>
        /// Videoplayer handling video playback
        /// </summary>
        private UnityEngine.Video.VideoPlayer m_videoPlayer;
        /// <summary>
        /// The currently displayed cross promotion item
        /// </summary>
        private CrossPromotionItem m_currentCrossPromotionItem;
        private RenderTexture m_renderTexture;
        
        [SerializeField]
        private Texture m_editorDummyTexture;

        /// <summary>
        /// You can use this render texture to display the cross promotion video in a custom way
        /// </summary>
        public RenderTexture RenderTexture
        {
            get
            {
                if (m_renderTexture == null)
                    m_renderTexture = CreateRenderTexture();
                return m_renderTexture;
            }
        }
        /// <summary>
        /// Use this property to change resolution of the video at runtime
        /// </summary>
        public Vector2Int Resolution
        {
            set
            {
                if (m_renderTexture)
                    m_renderTexture.Release();
                m_renderTexture = CreateRenderTexture(Mathf.Clamp(value.x, 50, 1080), Mathf.Clamp(value.y, 50, 1080));
            }
            get
            {
                return m_renderTexture ? new Vector2Int(m_renderTexture.width, m_renderTexture.height) : Vector2Int.zero;
            }
        }
        [Serializable]
        public class StringUnityEvent : UnityEvent<string> { }
        [Serializable]
        public class BoolUnityEvent : UnityEvent<bool> { }
        /// <summary>
        /// Event called when the video name changes
        /// </summary>
        public StringUnityEvent onVideoNameChanged;
        /// <summary>
        /// Event called when the url changes
        /// </summary>
        public StringUnityEvent onUrlChanged;
        /// <summary>
        /// Event called when the video ui should be displayed
        /// </summary>
        public BoolUnityEvent onVisibilityChange;

        private void Awake()
        {
            onVisibilityChange.Invoke(false);
            var videoplayer = GetComponent<UnityEngine.Video.VideoPlayer>();
            m_videoPlayer = videoplayer ? videoplayer : gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            m_videoPlayer.source = VideoSource.Url;
            m_videoPlayer.errorReceived += VideoPlayer_errorReceived;
            m_videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
            m_videoPlayer.isLooping = true;
            Resolution = new Vector2Int(256, 256);

            CrossPromotionManager.OnCrossPromotionInitialized += OnCrossPromotionInitialized;
        }

        /// <summary>
        /// Call this when the user clicks on the cross promo
        /// On Android : "market://details?id=YOUR_ID"
        /// On IOS : "itms-apps://itunes.apple.com/app/idYOUR_ID"
        /// </summary>
        public void OnClicked()
        {
#if UNITY_EDITOR
            Application.OpenURL("https://www.homagames.com/");
#else
            if (m_currentCrossPromotionItem != null)
            {
                CrossPromotionManager.OnCrossPromoItemClick(m_currentCrossPromotionItem.ClickUrl);

                // Reload cross promotion item after click
                StopVideo();
                AssignNextVideo();
                PlayVideo();
            }
#endif
        }

        private void OnCrossPromotionInitialized()
        {
#if UNITY_EDITOR
            HomaGamesLog.Debug("Displaying Editor Dummy Cross Promo Ad");
            onVideoNameChanged.Invoke("It works!");
            onVisibilityChange.Invoke(true);
            m_displayImage.texture = m_editorDummyTexture;
#else
            AssignNextVideo();
            PlayVideo();
#endif
        }

        private void AssignNextVideo()
        {
            if (CrossPromotionManager.TryGetNextCrossPromotionItem(out CrossPromotionItem item))
            {
                m_currentCrossPromotionItem = item;
                m_videoPlayer.url = item.LocalVideoFilePath;
                onVideoNameChanged.Invoke(item.Label);
                onUrlChanged.Invoke(item.ClickUrl);
                HomaGamesLog.Debug("Displaying Cross Promotion Ad : " + item.Name);
            }
        }

#region Video Player Callbacks
        private void VideoPlayer_prepareCompleted(UnityEngine.Video.VideoPlayer source)
        {
            PlayVideo();
        }

        private void VideoPlayer_errorReceived(UnityEngine.Video.VideoPlayer source, string message)
        {
            HomaGamesLog.Error("Cross Promotion Playback Error : " + message);
            StopVideo();
            AssignNextVideo();
            PlayVideo();
        }
#endregion

#region Utils
        private RenderTexture CreateRenderTexture(int width = 256, int height = 256)
        {
            var tex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, 0);
            if (m_videoPlayer)
                m_videoPlayer.targetTexture = tex;
            if (m_displayImage)
                m_displayImage.texture = tex;
            return tex;
        }
        private void PlayVideo()
        {
            if (m_videoPlayer.url != "")
            {
                if (m_videoPlayer.isPrepared && !m_videoPlayer.isPlaying)
                {
                    onVisibilityChange.Invoke(true);
                    m_videoPlayer.Play();
                }
                else
                    m_videoPlayer.Prepare();
            }
        }
        private void StopVideo()
        {
            if (m_videoPlayer.isPlaying)
                m_videoPlayer.Stop();
            onVisibilityChange.Invoke(false);
        }
#endregion

#region Unity LifeCycle
        private void OnEnable()
        {
            if(CrossPromotionManager.IsInitialized)
            {
                AssignNextVideo();
                PlayVideo();
            }
        }
        private void OnDisable()
        {
            StopVideo();
        }
        private void OnDestroy()
        {
            RenderTexture.Release();
        }
#endregion
    }
}
