using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace HomaGames.HomaBelly
{
    public class MobileDialog : MonoBehaviour
    {
        private List<ButtonWithCallback> ButtonList;
        [CanBeNull]
        private Action OnPopupCancel;

        #region Private helpers

        private static MobileDialog _cachedMobileDialog;
        private static MobileDialog InstantiateMobileDialog()
        {
            if (_cachedMobileDialog == null)
            {
                GameObject go = new GameObject("MobileDialog");
                DontDestroyOnLoad(go);
                _cachedMobileDialog = go.AddComponent<MobileDialog>();
            }

            return _cachedMobileDialog;
        }
        
        private static void SetupMobileDialog(List<ButtonWithCallback> buttonList, Action onCancel)
        {
            MobileDialog dialog = InstantiateMobileDialog();

            dialog.ButtonList = buttonList;
            dialog.OnPopupCancel = onCancel;
        }

        #endregion

        #region public interface

        /// <summary>
        /// Shows an native popup on screen
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="message">The text content of the popup</param>
        /// <param name="okButton">The button behaviour of the popup</param>
        /// <param name="OnCancel">[Android only] optional callback for when the popup is cancelled</param>
        [PublicAPI]
        public static void Create([NotNull] string title, [NotNull] string message,
            ButtonWithCallback okButton,
            Action OnCancel)
            => Create(title, message, okButton, true, OnCancel);
        
        /// <summary>
        /// Shows an native popup on screen
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="message">The text content of the popup</param>
        /// <param name="okButton">The button behaviour of the popup</param>
        /// <param name="cancellable">[Android only] whether or not clicking outside the
        /// popup can cancel it</param>
        /// <param name="OnCancel">[Android only] optional callback for when the popup is cancelled</param>
        [PublicAPI]
        public static void Create([NotNull] string title, [NotNull] string message, 
            ButtonWithCallback okButton, 
            bool cancellable = true, Action OnCancel = null)
        {
            SetupMobileDialog(
                new List<ButtonWithCallback> {okButton}, OnCancel);
            
            MobileNative.ShowInfo(title, message, okButton.Message, cancellable);
        }

        /// <summary>
        /// Shows an native popup on screen
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="message">The text content of the popup</param>
        /// <param name="acceptButton">The button behaviour of the first button of the popup. This button will
        /// be highlighted on iOS.</param>
        /// <param name="declineButton">The button behaviour of the second button of the popup</param>
        /// <param name="OnCancel">[Android only] optional callback for when the popup is cancelled</param>
        [PublicAPI]
        public static void Create([NotNull] string title, [NotNull] string message,
            ButtonWithCallback acceptButton, ButtonWithCallback declineButton,
            Action OnCancel)
            => Create(title, message, acceptButton, declineButton, true, OnCancel);
        
        /// <summary>
        /// Shows an native popup on screen
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="message">The text content of the popup</param>
        /// <param name="acceptButton">The button behaviour of the first button of the popup. This button will
        /// be highlighted on iOS.</param>
        /// <param name="declineButton">The button behaviour of the second button of the popup</param>
        /// <param name="cancellable">[Android only] whether or not clicking outside the
        /// popup can cancel it</param>
        /// <param name="OnCancel">[Android only] optional callback for when the popup is cancelled</param>
        [PublicAPI]
        public static void Create([NotNull] string title, [NotNull] string message, 
            ButtonWithCallback acceptButton, ButtonWithCallback declineButton, 
            bool cancellable = true, Action OnCancel = null)
        {
            SetupMobileDialog(
                new List<ButtonWithCallback> {acceptButton, declineButton}, OnCancel);
            
            MobileNative.ShowDialog(title, message, acceptButton.Message, declineButton.Message, cancellable);
        }

        /// <summary>
        /// Shows an native popup on screen
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="message">The text content of the popup</param>
        /// <param name="acceptButton">The button behaviour of the first button of the popup. This button will
        /// be highlighted on iOS.</param>
        /// <param name="neutralButton">The button behaviour of the second button of the popup.</param>
        /// <param name="declineButton">The button behaviour of the third button of the popup</param>
        /// <param name="OnCancel">[Android only] optional callback for when the popup is cancelled</param>
        [PublicAPI]
        public static void Create([NotNull] string title, [NotNull] string message,
            ButtonWithCallback acceptButton, ButtonWithCallback neutralButton, ButtonWithCallback declineButton,
            Action OnCancel)
            => Create(title, message, acceptButton, neutralButton, declineButton, true, OnCancel);
        
        /// <summary>
        /// Shows an native popup on screen
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="message">The text content of the popup</param>
        /// <param name="acceptButton">The button behaviour of the first button of the popup. This button will
        /// be highlighted on iOS.</param>
        /// <param name="neutralButton">The button behaviour of the second button of the popup.</param>
        /// <param name="declineButton">The button behaviour of the third button of the popup</param>
        /// <param name="cancellable">[Android only] whether or not clicking outside the
        /// popup can cancel it</param>
        /// <param name="OnCancel">[Android only] optional callback for when the popup is cancelled</param>
        [PublicAPI]
        public static void Create([NotNull] string title, [NotNull] string message, 
            ButtonWithCallback acceptButton, ButtonWithCallback neutralButton, ButtonWithCallback declineButton, 
            bool cancellable = true, Action OnCancel = null)
        {
            SetupMobileDialog(
                new List<ButtonWithCallback> {acceptButton, neutralButton, declineButton}, OnCancel);

            MobileNative.ShowDialog(title, message, 
                acceptButton.Message, neutralButton.Message, declineButton.Message, cancellable);
        }

        #endregion

        #region event listeners
        [UsedImplicitly]
        public void OnButtonClickCallback(string message)
        {
            if (!int.TryParse(message, NumberStyles.Integer, CultureInfo.InvariantCulture, out var buttonIndex))
            {
                HomaGamesLog.Error($"[MobileDialog] Cannot parse \"{message}\" as button index from native code.");
                return;
            }

            if (buttonIndex < 0 || buttonIndex >= ButtonList.Count)
            {
                HomaGamesLog.Error($"[MobileDialog] buttonIndex \"{buttonIndex}\" out of range.");
                return;
            }
            
            ButtonList[buttonIndex].Callback?.Invoke();
        }

        [UsedImplicitly]
        public void OnPopupCancelled(string _)
        {
            OnPopupCancel?.Invoke();
        }
        #endregion

        public struct ButtonWithCallback
        {
            [NotNull]
            public string Message;
            [CanBeNull]
            public Action Callback;
        }
    }
}