using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public abstract class TaskBase
    {
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }
        public Exception Exception { get; private set; }
        private Action OnCompletedAction;
    
    
        public Awaitable GetAwaiter()
        {
            return ConfigureAwait(true);
        }

        public Awaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            return new Awaitable(this, continueOnCapturedContext);
        }

        public static implicit operator Task(TaskBase taskBase)
        {
            return taskBase.ToTask();
        }

        private async Task ToTask()
        {
            await this;
        }

        protected void OnTaskCompleted()
        {
            IsCompleted = true;
            OnCompletedAction?.Invoke();
        }
    
        protected void OnTaskFailed(Exception exception)
        {
            IsFailed = true;
            Exception = exception;
            OnCompletedAction?.Invoke();
        }

        public class Awaitable : INotifyCompletion
        {
            private readonly bool CaptureContext;
            private TaskBase Parent { get; }
            // ReSharper disable once MemberCanBePrivate.Global
            public bool IsCompleted => Parent.IsCompleted;

            public Awaitable(TaskBase parent, bool captureContext)
            {
                CaptureContext = captureContext;
                Parent = parent;
            }

            public void GetResult()
            {
                if (Parent.IsFailed)
                    throw Parent.Exception;
            }

            public void OnCompleted(Action continuation)
            {
                if (IsCompleted)
                    continuation.Invoke();
                else
                {
                    SynchronizationContext capturedSynchronizationContext = SynchronizationContext.Current;
                    if (CaptureContext && capturedSynchronizationContext != null)
                    {
                        Parent.OnCompletedAction += () =>
                        {
                            capturedSynchronizationContext.Post(_ => continuation(), null);
                        };
                    }
                    else
                    {
                        Parent.OnCompletedAction += continuation;
                    }
                }
            }
        }
    }

    public abstract class TaskBase<T> : TaskBase
    {
        private T _result;
        public T Result
        {
            get
            {
                if (!IsCompleted)
                    throw new TaskNotCompletedException("Tried to access Result before task was completed.");
                if (IsFailed)
                    throw Exception;
            
                return _result;
            }
            private set => _result = value;
        }

        public new TypedAwaitable GetAwaiter()
        {
            return new TypedAwaitable(this, true);
        }
    
        public new TypedAwaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            return new TypedAwaitable(this, continueOnCapturedContext);
        }
        
        public static implicit operator Task<T>(TaskBase<T> taskBase)
        {
            return taskBase.ToTask();
        }

        private async Task<T> ToTask()
        {
            return await this;
        }

        protected void OnTaskCompleted(T value)
        {
            Result = value;
            base.OnTaskCompleted();
        }
    
        public class TypedAwaitable : Awaitable
        {
            private TaskBase<T> Parent { get; }

            public TypedAwaitable(TaskBase<T> parent, bool captureContext) : base(parent, captureContext)
            {
                Parent = parent;
            }

            public new T GetResult()
            {
                base.GetResult(); // Can throw
                return Parent.Result;
            }
        }

        public class TaskNotCompletedException : Exception
        {
            public TaskNotCompletedException(string message) : base(message)
            {
            
            }
        }
    }
}