using System;
using System.Threading;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections.Auxiliary
{
    internal class ThrottledAction : IDisposable, IControlledAction
    {
        private volatile Action _action = null;
        private TimeSpan _timeBetweenInvokations;
        private Task _actionTask = Task.CompletedTask;
        private bool _disposedValue;
        /// <summary>
        ///  How many actions have been queued since the last one
        ///  was executed.
        /// </summary>
        private int _queuedCounter = 0;

        public ThrottledAction() : this(TimeSpan.FromMilliseconds(20)) { }

        public ThrottledAction(TimeSpan timeBetweenInvokations)
        {
            _timeBetweenInvokations = timeBetweenInvokations;
        }

        public ThrottledAction(Action action, TimeSpan timeBetweenInvokations)
        {
            _action = action;
            _timeBetweenInvokations = timeBetweenInvokations;
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
        public bool InvokeAction(Action action)
        {
            _action = action;
            return InvokeAction();
        }

        /// <summary>
        /// Invokes the action on another thread at the appropriate time in the future
        /// Returns true if the action was queued
        /// </summary>
        public bool InvokeAction()
        {
            // Increment queue count, and do an equality check instead of a less
            // than check in case it rolls over. Odds should be low of hitting
            // exactly 1 in this instance.
            if (Interlocked.Increment(ref _queuedCounter) == 1)
            {
                Interlocked.Exchange(ref _actionTask, _actionTask.ContinueWith(_ =>
                {
                    _action?.Invoke();

                    // Do the throttle delay
                    Task.Delay(_timeBetweenInvokations).ContinueWith(__ =>
                    {
                        // If InvokeAction has been called while being throttled
                        // then invoke again. Reset counter to zero.
                        if (Interlocked.Exchange(ref _queuedCounter, 0) != 1)
                        {
                            InvokeAction();
                        }
                    });
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
