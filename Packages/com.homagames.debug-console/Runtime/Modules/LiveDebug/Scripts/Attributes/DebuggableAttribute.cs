using System;
using System.Diagnostics;
using UnityEngine.Scripting;

namespace HomaGames.HomaConsole.Core.Attributes
{
    public abstract class DebuggableAttribute : PreserveAttribute
    {
        /// <summary>
        /// Where is this property displayed in the debug UI
        /// </summary>
        public string[] Paths { get; protected set; }
        /// <summary>
        /// Replace the name of the field by a custom name
        /// </summary>
        public string CustomName = "";
        /// <summary>
        /// Lower number means higher in the hierarchy
        /// </summary>
        public int Order;
        
        public DebuggableAttribute()
        {
            Paths = Array.Empty<string>();
        }

        public DebuggableAttribute(params string[] paths)
        {
            Paths = paths;
        }
    }
}