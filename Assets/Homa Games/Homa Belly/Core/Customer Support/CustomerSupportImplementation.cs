using System;
using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    public abstract class CustomerSupportImplementation
    {
        /// <summary>
        /// Initialises the Customer Support implementation
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Check if the Customer Support is ready to be used.
        /// </summary>
        /// <returns>True if initialised</returns>
        public virtual bool IsInitialised()
        {
            return false;
        }

        /// <summary>
        /// Shows the FAQ
        /// </summary>
        public virtual void ShowFAQs()
        {
        }

        /// <summary>
        /// Use this to open the conversation window from the settings window
        /// </summary>
        public virtual void OpenConversationWindowFromSettings()
        {
        }

        /// <summary>
        /// Use this to open the conversation window from the rating popup
        /// </summary>
        public virtual void OpenConversationWindowFromRatePopup()
        {
        }

        /// <summary>
        /// Use this to open the conversation window with custom tags for context
        /// </summary>
        /// <param name="tags">List of tag to filter issues in the Customer Support dashboard</param>
        /// <param name="customMetadata">Add context to the issue with extra meta data</param>
        public virtual void OpenConversationWindow(string[] tags = null,Dictionary<string,object> customMetadata = null)
        {
        }

        /// <summary>
        /// Get the number of unread messages from support
        /// </summary>
        /// <param name="onComplete">Callback with the number of unread messages</param>
        public virtual void GetUnreadMessagesAsync(Action<int> onComplete)
        {
        }
    }
}