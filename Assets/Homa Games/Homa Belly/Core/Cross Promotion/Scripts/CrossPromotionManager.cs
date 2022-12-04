using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class CrossPromotionManager
    {
        private const int MIN_ITEMS_IN_QUEUE = 10;
        /// <summary>
        /// Stored video path so it can be used in threads
        /// </summary>
        private static string m_crossPromotionVideoPath = "";
        /// <summary>
        /// Full cross promotion folder path
        /// </summary>
        public static string CrossPromotionVideoPath
        {
            get { return m_crossPromotionVideoPath; }
            private set { m_crossPromotionVideoPath = Path.Combine(value, "cross_promotion/"); }
        }

        /// <summary>
        /// Concurrent Queue (FIFO) with the cross promotion items already selected
        /// depending on their configured weights. Cross promotion UI must Dequeue
        /// one item from this queue and display in on the screen.
        ///
        /// This Queue is guaranteed to contain sorted items after initialization (may
        /// take some seconds due to video download)
        /// </summary>
        private static ConcurrentQueue<CrossPromotionItem> CrossPromotionItemsConcurrentQueue;

        private static event Action onCrossPromotionInitialized;
        /// <summary>
        /// This Action will be invoked whenever the Cross Promotion is initialized
        /// and ready to display items
        /// </summary>
        public static event Action OnCrossPromotionInitialized
        {
            add
            {
                if (onCrossPromotionInitialized == null || !onCrossPromotionInitialized.GetInvocationList().Contains(value))
                {
                    onCrossPromotionInitialized += value;
                }

                // If already initialized when subscribing a new event, invoke immediately
                if (IsInitialized)
                {
                    value();
                }
            }

            remove
            {
                if (onCrossPromotionInitialized != null && onCrossPromotionInitialized.GetInvocationList().Contains(value))
                {
                    onCrossPromotionInitialized -= value;
                }
            }
        }

        private static RemoteConfiguration.RemoteConfigurationModelEveryTime _remoteConfigurationModel;
        private static CrossPromotionVideoDownloader videoDownloader = new CrossPromotionVideoDownloader();
        /// <summary>
        /// Keeps track of how many cross promo items have been impressed so far.
        /// This counter needs to be sent to servers for analytics purposes
        /// </summary>
        private static int impressionCounter = 0;
        public static bool IsInitialized = false;

        #region Public methods

        /// <summary>
        /// Initialize the Cross Promotion feature. This will read the cross promotion
        /// remote configuration fetched, sort all the cross promotion items by weight
        /// and download the very first one, making it ready for display.
        ///
        /// Whenever a cross promotion item is played on the screen, this manager
        /// will download a new video (if any remaining) and launch again the algorithm
        /// to randomly select items depending on their weights.
        /// </summary>
        /// <param name="configurationModel">The Remote Configuration fetched from server</param>
        public static void Initialize(RemoteConfiguration.RemoteConfigurationModelEveryTime configurationModel)
        {
            CrossPromotionVideoPath = Application.persistentDataPath;
            _remoteConfigurationModel = configurationModel;
            CrossPromotionItemsConcurrentQueue = new ConcurrentQueue<CrossPromotionItem>();

            // If there is no cross promotion data, do nothing
            if (!HasCrossPromotionData())
            {
                InitializationFinished();
                return;
            }

            DownloadAndFillQueue();
        }

        /// <summary>
        /// Get the next CrossPromotionItem to display.
        /// Once you call this function, you must display the cross promotion immediatly.
        /// </summary>
        /// <param name="crossPromotionItem">The next CrossPromotionItem to display</param>
        /// <returns>True if there is a cross promotion item to display</returns>
        public static bool TryGetNextCrossPromotionItem(out CrossPromotionItem crossPromotionItem)
        {
            if(CrossPromotionItemsConcurrentQueue.TryDequeue(out CrossPromotionItem item))
            {
                DownloadAndFillQueue();
                TrackImpression(item.ImpressionUrl);
                crossPromotionItem = item;
                return true;
            }
            crossPromotionItem = null;
            return false;
        }

        /// <summary>
        /// Opens the given URL after replacing the appropiate macros: {adindex},
        /// {adt0}, {adt1}...
        /// </summary>
        /// <param name="clickUrl">The ClickUrl of the cross promo item clicked</param>
        public static void OnCrossPromoItemClick(string clickUrl)
        {
            Application.OpenURL(ReplaceMacros(clickUrl));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if there is still a cross promotion item to be downloaded.
        /// If not, fills the queue direcly. If download is required, wait
        /// this to be completed in order to fill the queue.
        /// </summary>
        private static void DownloadAndFillQueue()
        {
            CrossPromotionItem nextItemToDownload = _remoteConfigurationModel.CrossPromotionConfigurationModel.RandomElementByWeightToBeDownloaded();
            // If an item to be downloaded is found
            if (nextItemToDownload != null)
            {
                // Trigger and wait for download. Then fulfill queue
                videoDownloader.DownloadItem(nextItemToDownload).ContinueWithOnMainThread(result =>
                {
                    // An error happened, move to initialized (if not already)
                    if (result.Exception != null)
                    {
                        HomaGamesLog.Error($"[Cross Promotion] Could not download item: {nextItemToDownload.ItemId}");
                        InitializationFinished();
                    }
                    else
                    {
                        // Download succeed, clear and fill the queue
                        CrossPromotionItemsConcurrentQueue = new ConcurrentQueue<CrossPromotionItem>();
                        FillQueueWithWeightedSelectedItems();
                    }
                });
            }
            else
            {
                // All items downloaded, fulfill queue
                FillQueueWithWeightedSelectedItems();
            }
        }

        /// <summary>
        /// Fills the cross promotion items queue in order to be consumed
        /// sequentially (FIFO). This queue fill runs in an asynchronous Task
        /// to avoid blocking the Main Thread.
        /// </summary>
        private static void FillQueueWithWeightedSelectedItems()
        {
            // If queue is full, do nothing
            if (CrossPromotionItemsConcurrentQueue.Count == MIN_ITEMS_IN_QUEUE)
            {
                return;
            }

            // Fill queue in an asynchronous Task, as we don't want to block the main thread
            Task.Run(() =>
            {
                while (CrossPromotionItemsConcurrentQueue.Count < MIN_ITEMS_IN_QUEUE)
                {
                    try
                    {
                        CrossPromotionItem item = _remoteConfigurationModel.CrossPromotionConfigurationModel.RandomElementByWeightAndLocalAvailability();

                        if (item != null)
                        {
                            HomaGamesLog.Debug($"[Cross Promotion] Enqueuing item: {item.AppId} - {item.Name} with weight {item.Weight}");
                            CrossPromotionItemsConcurrentQueue.Enqueue(item);
                        }
                        else
                        {
                            HomaGamesLog.Error($"[Cross Promotion] Can't fill the queue because there is no elements. This should never happen.");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Error($"[Cross Promotion] Exception enqueuing: {e.Message}");
                    }
                }
            }).ContinueWithOnMainThread((t)=>InitializationFinished());
        }

        /// <summary>
        /// Sets the cross promotion manager to the <c>initialized/c> status,
        /// invoking any required callback.
        /// </summary>
        private static void InitializationFinished()
        {
            if (!IsInitialized)
            {
                HomaGamesLog.Debug($"[Cross Promotion] Initialization success");
                IsInitialized = true;
                if (onCrossPromotionInitialized != null)
                {
                    onCrossPromotionInitialized.Invoke();
                }
            }
        }

        /// <summary>
        /// Determines if the remote configuration contains valid cross promotion
        /// data
        /// </summary>
        /// <returns></returns>
        private static bool HasCrossPromotionData()
        {
            return _remoteConfigurationModel != null
                && _remoteConfigurationModel.CrossPromotionConfigurationModel != null
                && _remoteConfigurationModel.CrossPromotionConfigurationModel.Items != null
                && _remoteConfigurationModel.CrossPromotionConfigurationModel.Items.Count > 0;
        }
        /// <summary>
        /// Simple impression tracking
        /// </summary>
        private static void TrackImpression(string impressionUrl)
        {
            try
            {
                using (HttpClient client = HttpCaller.GetHttpClient())
                {
                    client.GetAsync(ReplaceMacros(impressionUrl));
                    impressionCounter++;
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Error("Error tracking Cross Promotion Impression : " + e.Message);
            }
        }

        /// <summary>
        /// Replaces for the given URL all the required macros
        /// </summary>
        /// <param name="url">The URL to be processed</param>
        /// <returns>The URL with all the macros replaced</returns>
        private static string ReplaceMacros(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            // Replace known macros
            string result = url
                .Replace("{adindex}", impressionCounter.ToString())
                .Replace("{gaid}", Identifiers.Gaid)
                .Replace("{idfa}", Identifiers.Idfa)
                .Replace("{idfv}", Identifiers.Idfv)
                .Replace("{adt0}", "")
                .Replace("{adt1}", "")
                .Replace("{adt2}", "");

            // Finally, replace any new or unknown macro with empty value
            result = Regex.Replace(result, "\\{[^}]+\\}", "");
            return result;
        }
#endregion
    }
}