using System;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class CustomerSupport
    {
        /// <summary>
        /// Check if the Customer Support is ready to be used.
        /// </summary>
        /// <returns>True if initialised</returns>
        public static bool IsInitialised()
        {
            if (HomaBridgeDependencies.GetCustomerSupport(out CustomerSupportImplementation customerSupport))
                return customerSupport.IsInitialised();
            HomaGamesLog.Warning("No Customer Support Available.");
            return false;
        }

        /// <summary>
        /// Shows the FAQ
        /// </summary>
        public static void ShowFAQs()
        {
            if(HomaBridgeDependencies.GetCustomerSupport(out CustomerSupportImplementation customerSupport))
                customerSupport.ShowFAQs();
            else
                HomaGamesLog.Warning("No Customer Support Available.");
        }

        /// <summary>
        /// Use this to open the conversation window from the settings window
        /// </summary>
        public static void OpenConversationWindowFromSettings()
        {
            if(HomaBridgeDependencies.GetCustomerSupport(out CustomerSupportImplementation customerSupport))
                customerSupport.OpenConversationWindowFromSettings();
            else
                HomaGamesLog.Warning("No Customer Support Available.");
        }

        /// <summary>
        /// Use this to open the conversation window from the rating popup
        /// </summary>
        public static void OpenConversationWindowFromRatePopup()
        {
            if(HomaBridgeDependencies.GetCustomerSupport(out CustomerSupportImplementation customerSupport))
                customerSupport.OpenConversationWindowFromRatePopup();
            else
                HomaGamesLog.Warning("No Customer Support Available.");
        }

        /// <summary>
        /// Use this to open the conversation window with custom tags for context
        /// </summary>
        /// <param name="tags">List of tag to filter issues in the Customer Support dashboard</param>
        /// <param name="customMetadata">Add context to the issue with extra meta data</param>
        public static void OpenConversationWindow(string[] tags = null,Dictionary<string,object> customMetadata = null)
        {
            if(HomaBridgeDependencies.GetCustomerSupport(out CustomerSupportImplementation customerSupport))
                customerSupport.OpenConversationWindow(tags,customMetadata);
            else
                HomaGamesLog.Warning("No Customer Support Available.");
        }

        /// <summary>
        /// Get the number of unread messages from support
        /// </summary>
        /// <param name="onComplete">Callback with the number of unread messages</param>
        public static void GetUnreadMessagesAsync(Action<int> onComplete = null)
        {
            if(HomaBridgeDependencies.GetCustomerSupport(out CustomerSupportImplementation customerSupport))
                customerSupport.GetUnreadMessagesAsync(onComplete);
            else
                HomaGamesLog.Warning("No Customer Support Available.");
        }
    }
}