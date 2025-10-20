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
        class TestControlledActionLatency : ExtendedNotifyPropertyChanged
        {
            IControlledAction _controlledAction;

            public TestControlledActionLatency(IControlledAction controlledControl)
            {
                _controlledAction = controlledControl;
                _controlledAction.SetAction(() => RaisePropertyChanged(nameof(StoredValue)));
            }

            private int _storedValue = 0;
            public int StoredValue
            {
                get => _storedValue;
                set
                {
                    _storedValue = value;
                    _controlledAction.InvokeAction();
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
            while ((DateTime.Now - start) < TimeSpan.FromSeconds(1))
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
                throttledAction.InvokeAction(() =>
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
            TestThreadLatency(new ThrottledAction(TimeSpan.FromMilliseconds(20)));
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
            TestThreadLatency(new OldThrottledAction(TimeSpan.FromMilliseconds(20)));
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
            TestThreadLatency(new UnthrottledAction());
        }

        private void TestThreadLatency(IControlledAction action)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var startingLine = new ManualResetEvent(false);
            var threads = new List<Thread>();
            int eventCount = 0;
            int threadsStartedCount = 0;
            for (int i = 0; i < 800; ++i)
            {
                // Create a new thread instead of using the threadpool
                var thread = new Thread(new ThreadStart(() =>
                {
                    startingLine.WaitOne();
                    var token = cancellationTokenSource.Token;
                    int startValue = 0;
                    var testNew = new TestControlledActionLatency(action);
                    testNew.PropertyChanged += (s, e) => Interlocked.Increment(ref eventCount);
                    Interlocked.Increment(ref threadsStartedCount);
                    while (!token.IsCancellationRequested)
                    {
                        testNew.StoredValue = startValue++;
                    }
                }));
                thread.Start();
                threads.Add(thread);
            }
            startingLine.Set();

            // Wait a bit
            Thread.Sleep(5000);
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
            Debug.WriteLine($"Events Fired = {eventCount}");
            Debug.WriteLine($"Threads Started = {threadsStartedCount}");
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        /// <summary>
        /// Testing creating a ThrottledAction from the name. Relevant to serialization,
        /// but more relevant to UnthrottledAction.
        /// </summary>
        [TestMethod]
        public void TestCreatingThrottledActionFromName()
        {
            string typeName = typeof(ThrottledAction).FullName;
            Type type = null;

            foreach (var assemlby in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!assemlby.FullName.StartsWith("System") && !assemlby.FullName.StartsWith("Microsoft"))
                    {
                        var types = assemlby.GetTypes();
                        type = types.FirstOrDefault(t => t.FullName == typeName);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
            Assert.IsNotNull(type, $"Couldn't create ThrottledAction from name {typeName}");
            Assert.IsTrue(Activator.CreateInstance(type) is ThrottledAction, $"Couldn't create ThrottledAction from name {typeName}");
        }

        /// <summary>
        /// Testing creating a ThrottledAction from the name. Relevant to serialization.
        /// </summary>
        [TestMethod]
        public void TestCreatingUnthrottledActionFromName()
        {
            string typeName = typeof(UnthrottledAction).FullName;
            Type type = null;

            foreach (var assemlby in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!assemlby.FullName.StartsWith("System") && !assemlby.FullName.StartsWith("Microsoft"))
                    {
                        var types = assemlby.GetTypes();
                        type = type ?? types.FirstOrDefault(t => t.FullName == typeName);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
            Assert.IsNotNull(type, $"Couldn't create UnthrottledAction from name {typeName}");
            Assert.IsTrue(Activator.CreateInstance(type) is UnthrottledAction, $"Couldn't create UnthrottledAction from name {typeName}");
        }

    }
}
