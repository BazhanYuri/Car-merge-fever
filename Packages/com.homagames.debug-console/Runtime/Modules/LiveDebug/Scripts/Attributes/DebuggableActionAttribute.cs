using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace HomaGames.HomaConsole.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class DebuggableActionAttribute : DebuggableAttribute
    {
        public DebuggableActionAttribute()
        {
            Paths = Array.Empty<string>();
        }

        public DebuggableActionAttribute(params string[] paths)
        {
            Paths = paths;
        }
    }
}