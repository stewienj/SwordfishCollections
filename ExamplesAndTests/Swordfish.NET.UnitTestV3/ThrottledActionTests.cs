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
        /// Throttled Action ensures the last action that
        /// was invoked gets executed.
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

            // Wait for execution to complete
            Thread.Sleep(40);
            Assert.AreEqual(callCount, lastCallCountUpdated);
        }

        class TestLatencyNew : ExtendedNotifyPropertyChanged
        {
            ThrottledAction _throttledAction;

            public TestLatencyNew()
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

        class TestLatencyOld : ExtendedNotifyPropertyChanged
        {
            OldThrottledAction _throttledAction;

            public TestLatencyOld()
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


        [TestMethod]
        public void TestThreadPoolLatencyNew()
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
                    var testNew = new TestLatencyNew();
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

        [TestMethod]
        public void TestThreadPoolLatencyOld()
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
                    var testNew = new TestLatencyOld();
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
