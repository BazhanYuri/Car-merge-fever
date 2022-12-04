using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HomaGames.HomaBelly
{
    public static class ResourcesUtils
    {
        /// <summary>
        /// Is the same as <see cref="Resources.LoadAsync&lt;T&gt;">Resources.LoadAsync&lt;T&gt;</see>,
        /// but returns an awaitable object instead of working with events. 
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ResourceLoadTask<T> LoadAsync<T>(string resourcesPath) where T : Object
        {
            return new ResourceLoadTask<T>(Resources.LoadAsync<T>(resourcesPath));
        }
    }

    public class ResourceLoadTask<T> : TaskBase<T> where T : Object
    {
        private readonly ResourceRequest ResourceRequest;
    
        public ResourceLoadTask(ResourceRequest wrappedRequest)
        {
            ResourceRequest = wrappedRequest;
            ResourceRequest.completed += OnParentOperationCompleted;
        }

        private void OnParentOperationCompleted(AsyncOperation _)
        {
            OnTaskCompleted((T) ResourceRequest.asset);
        }
    }
}