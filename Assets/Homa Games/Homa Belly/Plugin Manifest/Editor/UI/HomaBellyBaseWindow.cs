using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public abstract class HomaBellyBaseWindow
    {
        public enum ID
        {
            NONE,
            INSTALL,
            INSTALLATION_PROGRESS
        }

        #region Private properties
        private bool visible = false;
        #endregion

        protected IHomaBellyWindowController homaBellyWindowController;

        #region Public methods

        public virtual void SetWindowController(IHomaBellyWindowController controller)
        {
            this.homaBellyWindowController = controller;
        }

        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }

        public void OnGUI(Rect windowPosition)
        {
            if (visible)
            {
                Draw(windowPosition);
            }
        }

        public void OnFocus()
        {
            if (visible)
            {
                OnVisibleFocus();
            }
        }

        #endregion

        #region Override
        protected abstract void Draw(Rect windowPosition);
        protected abstract void OnVisibleFocus();
        #endregion
    }
}
