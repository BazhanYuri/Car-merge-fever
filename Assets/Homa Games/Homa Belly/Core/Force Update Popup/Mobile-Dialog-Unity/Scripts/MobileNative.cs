using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    internal static class MobileNative
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogNeutral(string title, string message, string accept, string neutral, string decline);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogConfirm(string title, string message, string yes, string no);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogInfo(string title, string message, string ok);

        [DllImport("__Internal")]
        private static extern void _TAG_DismissCurrentAlert();
#endif
        
#if UNITY_ANDROID
        private static AndroidJavaClass JavaBridgeClass = new AndroidJavaClass("com.damysus.nativepopup.Bridge");
#endif

        [PublicAPI]
        public static void ShowDialog(string title, string message, string accept, string neutral, string decline, bool cancelable = true)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogNeutral(title, message, accept, neutral, decline);
#elif UNITY_ANDROID            
            JavaBridgeClass.CallStatic("ShowDialog", title, message, accept, neutral, decline, cancelable);
#endif
        }

        /// <summary>
        /// Calls a Native Confirm Dialog on iOS and Android
        /// </summary>
        /// <param name="title">Dialog title text</param>
        /// <param name="message">Dialog message text</param>
        /// <param name="yes">Accept Button text</param>
        /// <param name="no">Cancel Button text</param>
        /// <param name="cancelable">Android only. Allows setting the cancelable property of the dialog</param>
        [PublicAPI]
        public static void ShowDialog(string title, string message, string yes, string no, bool cancelable = true)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogConfirm(title, message, yes, no);
#elif UNITY_ANDROID            
            JavaBridgeClass.CallStatic("ShowDialog", title, message, yes, no, cancelable);
#endif
        }

        [PublicAPI]
        public static void ShowInfo(string title, string message, string ok, bool cancelable = true)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogInfo(title, message, ok);
#elif UNITY_ANDROID            
            JavaBridgeClass.CallStatic("ShowDialog", title, message, ok, cancelable);
#endif
        }

        [PublicAPI]
        public static void DismissCurrentAlert()
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_DismissCurrentAlert();
#elif UNITY_ANDROID
            JavaBridgeClass.CallStatic("DismissCurrentAlert");
#endif
        }
    }
}