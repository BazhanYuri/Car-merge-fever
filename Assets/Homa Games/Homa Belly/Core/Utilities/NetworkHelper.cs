using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class listening to network availability changes and notifying
    /// registerd events
    /// </summary>
    public class NetworkHelper
    {
        private const int REACHABILITY_CHECK_INTERVAL_IN_SECONDS = 10;

        /// <summary>
        /// Callback to be notified when a network reachability change is detected.
        /// This will be invoked in the Main Thread
        /// </summary>
        public event Action<NetworkReachability> OnNetworkReachabilityChange;

        /// <summary>
        /// Callback to be notified when a network reachability is first fetched.
        /// This will be invoked in the Main Thread
        /// </summary>
        public event Action<NetworkReachability> OnInitialNetworkReachabilityFetched;
        
        private bool stopListeningRequested = false;
        private bool initialValueFetched = false;
        private NetworkReachability lastReachabilityDetected;

        /// <summary>
        /// Detect a network rechability change and invokes the registered callback
        /// </summary>
        private void CheckReachabilityChange()
        {
            // Invoke very first time
            if (!initialValueFetched)
            {
                initialValueFetched = true;
                lastReachabilityDetected = Application.internetReachability;
                OnInitialNetworkReachabilityFetched?.Invoke(lastReachabilityDetected);
            }
            // Invoke if the network reachability has changed
            else if (lastReachabilityDetected != Application.internetReachability)
            {
                lastReachabilityDetected = Application.internetReachability;

                // Invoke change
                OnNetworkReachabilityChange?.Invoke(lastReachabilityDetected);
            }
        }

        /// <summary>
        /// Schedule a reachablility check after REACHABILITY_CHECK_INTERVAL_IN_SECONDS seconds.
        /// The delay happens asynchronously while the continuation happens
        /// in Unity Main Thread in order to invoke OnNetworkReachabilityChange callback
        /// </summary>
        private async void ReachabilityTask()
        {
            if (stopListeningRequested)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(REACHABILITY_CHECK_INTERVAL_IN_SECONDS));
            
            CheckReachabilityChange();
            // Schedule next reachability check
            ReachabilityTask();
        }

        /// <summary>
        /// Starts listening for network reachability changes
        /// </summary>
        public void StartListening()
        {
            stopListeningRequested = false;

            // Schedule asynchronous check in the future
            ReachabilityTask();
        }

        /// <summary>
        /// Stops listening for network reachability changes
        /// </summary>
        public void StopListening()
        {
            stopListeningRequested = true;
        }
    }
}