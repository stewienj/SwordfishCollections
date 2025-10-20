using System;
using System.Threading;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections.Auxiliary
{
    /// <summary>
    /// An implementation of ThrottledAction that uses a TaskDelay for the throttle
    /// </summary>
    internal class ThrottledActionTaskDelay : IControlledAction
    {
        private volatile Action _action = null;
        private TimeSpan _timeBetweenInvokations;
        /// <summary>
        ///  How many actions have been queued since the last one
        ///  was executed.
        /// </summary>
        private int _queuedCounter = 0;

        public ThrottledActionTaskDelay() : this(TimeSpan.FromMilliseconds(20)) { }

        public ThrottledActionTaskDelay(TimeSpan timeBetweenInvokations)
        {
            _timeBetweenInvokations = timeBetweenInvokations;
        }

        public ThrottledActionTaskDelay(Action action, TimeSpan timeBetweenInvokations)
        {
            _action = action;
            _timeBetweenInvokations = timeBetweenInvokations;
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
                Task.Run(()=>
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
                });
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
