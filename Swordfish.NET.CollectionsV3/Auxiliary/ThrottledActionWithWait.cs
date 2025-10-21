using System;
using System.Threading;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections.Auxiliary
{
    public class ThrottledActionWithWait : IDisposable, IControlledAction
    {
        private volatile Action _action;
        private TimeSpan _timeBetweenInvokations;
        private Task _actionTask = Task.CompletedTask;
        private int _actionsQueued = 0;
        private bool _disposedValue;

        public ThrottledActionWithWait(Action action, TimeSpan timeBetweenInvokations)
        {
            _action = action;
            _timeBetweenInvokations = timeBetweenInvokations;
        }

        public ThrottledActionWithWait(TimeSpan timeBetweenInvokations)
        {
            _timeBetweenInvokations = timeBetweenInvokations;
        }

        public ThrottledActionWithWait() : this(TimeSpan.FromMilliseconds(20))
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _actionTask = Task.CompletedTask;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetAction(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// Invokes the action on another thread at the appropriate time in the future
        /// Returns true if the action was queued
        /// </summary>
        public bool InvokeAction() => InvokeAction(_action);

        /// <summary>
        /// Invokes the action on another thread at the appropriate time in the future
        /// Returns true if the action was queued
        /// </summary>
        public bool InvokeAction(Action action)
        {
            if (_actionsQueued < 1)
            {
                //Interlocked gaurantees that a single thread is accessing the ref value at a time
                Interlocked.Increment(ref _actionsQueued);
                Interlocked.Exchange(ref _actionTask, _actionTask.ContinueWith(_ =>
                {
                    Interlocked.Decrement(ref _actionsQueued);
                    try
                    {
                        action?.Invoke();
                    }
                    finally
                    {
                        // Release any external objects held by this task
                        action = null;
                        // Threadsafe method of waiting
                        using (var manualResetEvent = new ManualResetEvent(false))
                        {
                            manualResetEvent.WaitOne(_timeBetweenInvokations);
                        }
                    }
                }));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
