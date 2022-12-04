using System;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// This is a helper class to run code on the main thread.
    /// </summary>
    public static class ThreadUtils
    {
        private static SynchronizationContext MainThreadSynchronizationContext;
        private static TaskScheduler MainThreadTaskScheduler;
        private static int MainThreadId;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            if (MainThreadSynchronizationContext == null
                || MainThreadTaskScheduler == null)
            {
                MainThreadId = Thread.CurrentThread.ManagedThreadId;
                MainThreadSynchronizationContext = SynchronizationContext.Current;
                MainThreadTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
        }

        static ThreadUtils()
        {
            Initialize();
        }
    
        /// <summary>
        /// This method will run the given action on the main thread.
        /// If the current thread is the main one, it will run it synchronously.
        /// If not, it will run it asynchronously, in a run-and-forget fashion.
        /// </summary>
        /// <param name="action">The action to run on the main thread.</param>
        public static void RunOnMainThreadCurrentOrForget(Action action)
        {        
            if (IsCurrentThreadMainThread())
            {
                action.Invoke();
                return;
            }

            RunAndForgetOnMainThread(action);
        }
    
        /// <summary>
        /// This method will run the given action asynchronously on the main thread.
        /// </summary>
        /// <param name="action">The action to run on the main thread.</param>
        public static void RunAndForgetOnMainThread(Action action)
        {
            MainThreadSynchronizationContext.Post(_ => { action(); }, null);
        }
    
        /// <summary>
        /// This method will run the given action on the main thread, and wait for it to be finished
        /// before returning.<br /><br />
        /// If called from the main thread, the action will be executed directly.
        /// </summary>
        /// <param name="action">The action to run on the main thread.</param>
        public static void RunOnMainThread(Action action)
        {
            if (IsCurrentThreadMainThread())
            {
                action.Invoke();
                return;
            }
        
            MainThreadSynchronizationContext.Send(_ => { action(); }, null);
        }

        /// <summary>
        /// This method will run the given function on the main thread, wait for it to finish,
        /// and return the result.<br /><br />
        /// If called from the main thread, the function will be executed directly.
        /// </summary>
        /// <param name="func">The function to execute on the main thread.</param>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>The return value of the given function.</returns>
        public static TResult RunOnMainThread<TResult>(Func<TResult> func)
        {
            if (IsCurrentThreadMainThread())
                return func();
        
            TResult returnValue = default(TResult);
            MainThreadSynchronizationContext.Send(x =>
                {
                    returnValue = func();
                }
                , null);
            return returnValue;
        }

        /// <summary>
        /// This method will run the given action on the main thread asynchronously.<br /><br />
        /// </summary>
        /// <param name="action">The action to execute on the main thread.</param>
        /// <returns>A task representing the execution of the action.</returns>
        public static Task RunOnMainThreadAsync(Action action) => RunOnMainThreadAsync(action, CancellationToken.None);

        /// <summary>
        /// This method will run the given action on the main thread asynchronously.<br /><br />
        /// </summary>
        /// <param name="action">The action to execute on the main thread.</param>
        /// <param name="cancellationToken">An associated cancellation token</param>
        /// <returns>A task representing the execution of the action.</returns>
        public static Task RunOnMainThreadAsync(Action action, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(action, cancellationToken, TaskCreationOptions.None, MainThreadTaskScheduler);
        }

        /// <summary>
        /// This method will run the given function on the main thread asynchronously.<br /><br />
        /// If called from the main thread, the function will be executed directly.
        /// </summary>
        /// <param name="func">The function to execute on the main thread.</param>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>A task representing the execution of the function.</returns>
        public static Task<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> func) =>
            RunOnMainThreadAsync(func, CancellationToken.None);

        /// <summary>
        /// This method will run the given function on the main thread asynchronously.<br /><br />
        /// If called from the main thread, the function will be executed directly.
        /// </summary>
        /// <param name="func">The function to execute on the main thread.</param>
        /// <param name="cancellationToken">A cancellation token to associate with the task.</param>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>A task representing the execution of the function.</returns>
        public static Task<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(func, cancellationToken, TaskCreationOptions.None, MainThreadTaskScheduler);
        }

        #region ContinueWith extensions
        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="func">The function to execute after the task.</param>
        /// <typeparam name="TSource">The return type of the task.</typeparam>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>A task describing the whole process.</returns>
        public static Task<TResult> ContinueWithOnMainThread<TSource, TResult>(this Task<TSource> task, Func<Task<TSource>, TResult> func) =>
            task.ContinueWithOnMainThread(func, CancellationToken.None);

        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="func">The function to execute after the task.</param>
        /// <param name="token">A cancellation token associated to the function execution.</param>
        /// <typeparam name="TSource">The return type of the task.</typeparam>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>A task describing the whole process.</returns>
        public static Task<TResult> ContinueWithOnMainThread<TSource, TResult>(this Task<TSource> task, Func<Task<TSource>, TResult> func, CancellationToken token)
        {
            return task.ContinueWith(func, token, TaskContinuationOptions.None, MainThreadTaskScheduler);
        } 
    
        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="action">The action to execute after the task.</param>
        /// <typeparam name="TSource">The return type of the task.</typeparam>
        /// <returns>A task describing the whole process.</returns>
        public static Task ContinueWithOnMainThread<TSource>(this Task<TSource> task, Action<Task<TSource>> action) =>
            task.ContinueWithOnMainThread(action, CancellationToken.None);

        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="action">The action to execute after the task.</param>
        /// <param name="token">A cancellation token associated to the action execution.</param>
        /// <typeparam name="TSource">The return type of the task.</typeparam>
        /// <returns>A task describing the whole process.</returns>
        public static Task ContinueWithOnMainThread<TSource>(this Task<TSource> task, Action<Task<TSource>> action, CancellationToken token)
        {
            return task.ContinueWith(action, token, TaskContinuationOptions.None, MainThreadTaskScheduler);
        } 
    
        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="func">The function to execute after the task.</param>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>A task describing the whole process.</returns>
        public static Task<TResult> ContinueWithOnMainThread<TResult>(this Task task, Func<Task, TResult> func) =>
            task.ContinueWithOnMainThread(func, CancellationToken.None);

        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="func">The function to execute after the task.</param>
        /// <param name="token">A cancellation token associated to the function execution.</param>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <returns>A task describing the whole process.</returns>
        public static Task<TResult> ContinueWithOnMainThread<TResult>(this Task task, Func<Task, TResult> func, CancellationToken token)
        {
            return task.ContinueWith(func, token, TaskContinuationOptions.None, MainThreadTaskScheduler);
        }

        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="action">The action to execute after the task.</param>
        /// <returns>A task describing the whole process.</returns>
        public static Task ContinueWithOnMainThread(this Task task, Action<Task> action) =>
            task.ContinueWithOnMainThread(action, CancellationToken.None);

        /// <summary>
        /// Acts like ContinueWith, but with the execution on the main thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="action">The action to execute after the task.</param>
        /// <param name="token">A cancellation token associated to the action execution.</param>
        /// <returns>A task describing the whole process.</returns>
        public static Task ContinueWithOnMainThread(this Task task, Action<Task> action, CancellationToken token)
        {
            return task.ContinueWith(action, token, TaskContinuationOptions.None, MainThreadTaskScheduler);
        } 
        #endregion

        private static bool IsCurrentThreadMainThread()
        {
            return Thread.CurrentThread.ManagedThreadId == MainThreadId;
        }
    }
}