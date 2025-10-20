using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System.Linq;
using Swordfish.NET.Collections.Auxiliary;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class ThrottledActionTests
    {
        // ********************************************************************
        // Nested Classes
        // ********************************************************************
        #region Nested Classes

        /// <summary>
        /// Class used for testing the thread pool latency when using the new
        /// code from ThrottledAction
        /// </summary>
        class TestLatencyNewThrottle : ExtendedNotifyPropertyChanged
        {
            ThrottledAction _throttledAction;

            public TestLatencyNewThrottle()
            {
                _throttledAction = new ThrottledAction(() => RaisePropertyChanged(nameof(StoredValue)), TimeSpan.FromMilliseconds(20));
            }

            private int _storedValue = 0;
            public int StoredValue
            {
                get => _storedValue;
                set
                {
                    _storedValue = value;
                    _throttledAction.InvokeAction();
                }
            }
        }

        /// <summary>
        /// Class used for testing the thread pool latency when using the old
        /// code from ThrottledAction
        /// </summary>
        class TestLatencyOldThrottle : ExtendedNotifyPropertyChanged
        {
            OldThrottledAction _throttledAction;

            public TestLatencyOldThrottle()
            {
                _throttledAction = new OldThrottledAction(() => RaisePropertyChanged(nameof(StoredValue)), TimeSpan.FromMilliseconds(20));
            }

            private int _storedValue = 0;
            public int StoredValue
            {
                get => _storedValue;
                set
                {
                    _storedValue = value;
                    _throttledAction.InvokeAction();
                }
            }
        }

        /// <summary>
        /// Class used for testing the thread pool latency when an
        /// action directly without a throttle
        /// </summary>
        class TestLatencyNoThrottle : ExtendedNotifyPropertyChanged
        {
            Action _action;

            public TestLatencyNoThrottle()
            {
                _action = () => RaisePropertyChanged(nameof(StoredValue));
            }

            private int _storedValue = 0;
            public int StoredValue
            {
                get => _storedValue;
                set
                {
                    _storedValue = value;
                    _action.Invoke();
                }
            }
        }

        #endregion Nested Classes

        /// <summary>
        /// This tests that actions are being throttled by the ThrottledAction
        /// class. Also tests throughput is reasonable.
        /// </summary>
        [TestMethod]
        public void TestTiming()
        {
            int updateCount = 0;
            var throttledAction = new ThrottledAction(() =>
            {
                Interlocked.Increment(ref updateCount);
            }, TimeSpan.FromMilliseconds(30));

            var start = DateTime.Now;
            int callCount = 0;
            while((DateTime.Now-start)<TimeSpan.FromSeconds(1))
            {
                callCount++;
                throttledAction.InvokeAction();
            }

            // Check the updates are throttled
            Assert.IsTrue(updateCount < 40);
            // Check there is reasonable throughput
            Assert.IsTrue(callCount > 100_000);
        }

        /// <summary>
        /// Tests that ThrottledAction executes the last action 
        /// invoked.
        /// </summary>
        [TestMethod]
        public void TestFinalActionExecuted()
        {
            int callCount = 0;
            var throttledAction = new ThrottledAction(null, TimeSpan.FromMilliseconds(30));
            int lastCallCountUpdated = 0;

            var start = DateTime.Now;
            while ((DateTime.Now - start) < TimeSpan.FromSeconds(1))
            {
                int local = ++callCount;
                throttledAction.InvokeAction(()=>
                {
                    Interlocked.Exchange(ref lastCallCountUpdated, local);
                });
            }

            // Wait a while for execution to complete
            Thread.Sleep(40);
            Assert.AreEqual(callCount, lastCallCountUpdated);
        }

        /// <summary>
        /// When this was written the implementation of ThrottledAction
        /// had been changed. This tests the threadpool latency when
        /// using the new implementation. The latency will be written
        /// out to the console, not actually used in a test.
        /// </summary>
        [TestMethod]
        public void TestThreadPoolLatencyNewThrotle()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var startingLine = new ManualResetEvent(false);
            for (int i = 0; i < 800; ++i)
            {
                // Create a new thread instead of using the threadpool
                var thread = new Thread(new ThreadStart(() =>
                {
                    startingLine.WaitOne();
                    var token = cancellationTokenSource.Token;
                    int startValue = 0;
                    var testNew = new TestLatencyNewThrottle();
                    while (!token.IsCancellationRequested)
                    {
                        testNew.StoredValue = startValue++;
                    }
                }));
                thread.Start();
            }
            startingLine.Set();

            // Wait a bit
            Thread.Sleep(1000);
            var resetEvent = new ManualResetEvent(false);
            var start = DateTime.Now;
            Task.Run(() =>
            {
                resetEvent.Set();
            });
            resetEvent.WaitOne();
            var duration = DateTime.Now - start;
            cancellationTokenSource.Cancel();
            Debug.WriteLine($"Task Run Latency = {duration}");
        }

        /// <summary>
        /// When this was written the implementation of ThrottledAction
        /// had been changed. This tests the threadpool latency when
        /// using the old implementation. The latency will be written
        /// out to the console, not actually used in a test.
        /// </summary>
        [TestMethod]
        public void TestThreadPoolLatencyOldThrottle()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var startingLine = new ManualResetEvent(false);
            for (int i = 0; i < 800; ++i)
            {
                // Create a new thread instead of using the threadpool
                var thread = new Thread(new ThreadStart(() =>
                {
                    startingLine.WaitOne();
                    var token = cancellationTokenSource.Token;
                    int startValue = 0;
                    var testNew = new TestLatencyOldThrottle();
                    while (!token.IsCancellationRequested)
                    {
                        testNew.StoredValue = startValue++;
                    }
                }));
                thread.Start();
            }
            startingLine.Set();

            // Wait a bit
            Thread.Sleep(1000);
            var resetEvent = new ManualResetEvent(false);
            var start = DateTime.Now;
            Task.Run(() =>
            {
                resetEvent.Set();
            });
            resetEvent.WaitOne();
            var duration = DateTime.Now - start;
            cancellationTokenSource.Cancel();
            Debug.WriteLine($"Task Run Latency = {duration}");
        }

        /// <summary>
        /// When this was written the implementation of ThrottledAction
        /// had been changed. This tests the threadpool latency when
        /// using no throttling. The latency will be written
        /// out to the console, not actually used in a test.
        /// </summary>
        [TestMethod]
        public void TestThreadLatencyNoThrottle()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var startingLine = new ManualResetEvent(false);
            for (int i = 0; i < 800; ++i)
            {
                // Create a new thread instead of using the threadpool
                var thread = new Thread(new ThreadStart(() =>
                {
                    startingLine.WaitOne();
                    var token = cancellationTokenSource.Token;
                    int startValue = 0;
                    var testNew = new TestLatencyNoThrottle();
                    while (!token.IsCancellationRequested)
                    {
                        testNew.StoredValue = startValue++;
                    }
                }));
                thread.Start();
            }
            startingLine.Set();

            // Wait a bit
            Thread.Sleep(1000);
            var resetEvent = new ManualResetEvent(false);
            var start = DateTime.Now;
            Task.Run(() =>
            {
                resetEvent.Set();
            });
            resetEvent.WaitOne();
            var duration = DateTime.Now - start;
            cancellationTokenSource.Cancel();
            Debug.WriteLine($"Task Run Latency = {duration}");
        }

    }
}
