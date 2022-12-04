using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace HomaGames.HomaConsole.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class DebuggableClassAttribute : PreserveAttribute
    {
    }
}