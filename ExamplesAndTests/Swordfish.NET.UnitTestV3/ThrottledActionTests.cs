using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        public void TestTimingThrottledActionWithWait()
        {
            int updateCount = 0;
            var throttledAction = new ThrottledActionWithWait(() =>
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
            Assert.IsTrue(updateCount < 40, $"Update count should be less than 40, actual count = {updateCount}");
            // Check there is reasonable throughput
            Assert.IsTrue(callCount > 100_000);
        }

        /// <summary>
        /// This tests that actions are being throttled by the ThrottledAction
        /// class. Also tests throughput is reasonable.
        /// </summary>
        [TestMethod]
        public void TestTimingThrottledActionTaskDelay()
        {
            int updateCount = 0;
            var throttledAction = new ThrottledActionTaskDelay(() =>
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
            Assert.IsTrue(updateCount < 40, $"Update count should be less than 40, actual count = {updateCount}");
            // Check there is reasonable throughput
            Assert.IsTrue(callCount > 100_000, $"Call count in low = {callCount}");
        }


        /// <summary>
        /// Tests that ThrottledAction executes the last action 
        /// invoked. Slightly different test to the one for
        /// ThrottledActionTaskDelay because the Task Delay
        /// version executes the actual final task.
        /// </summary>
        [TestMethod]
        public void TestFinalActionExecutedThrottledActionWithWait()
        {
            int callCount = 0;
            var throttledAction = new ThrottledActionWithWait(null, TimeSpan.FromMilliseconds(30));
            DateTime lastQueueTime = DateTime.Now;
            DateTime lastExecuteTime = DateTime.Now;

            var start = DateTime.Now;
            while ((DateTime.Now - start) < TimeSpan.FromSeconds(1))
            {
                int local = ++callCount;
                throttledAction.InvokeAction(() =>
                {
                    lastExecuteTime = DateTime.Now;
                });
                lastQueueTime = DateTime.Now;
            }

            // Wait a while for execution to complete
            Thread.Sleep(40);
            Assert.IsTrue(lastExecuteTime > lastQueueTime);
            Debug.WriteLine($"Last Execute Time - Last Queue Time = {lastExecuteTime - lastQueueTime}");
        }

        /// <summary>
        /// Tests that ThrottledAction executes the last action 
        /// invoked.
        /// </summary>
        [TestMethod]
        public void TestFinalActionExecutedThrottledActionTaskDelay()
        {
            int callCount = 0;
            var throttledAction = new ThrottledActionTaskDelay(null, TimeSpan.FromMilliseconds(30));
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
        /// Tests that the controlledAction parameter correctly changes the behaviour of the collection.
        /// </summary>
        [TestMethod]
        public void TestControlledActionParamter()
        {
            int propertyChangedInvocations = 0;
            void propertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == nameof(ConcurrentObservableCollection<int>.CollectionView))
                {
                    propertyChangedInvocations++;
                }
            }

            // No throttling
            propertyChangedInvocations = 0;
            var noThrottleCollection = new ConcurrentObservableCollection<int>(controlledAction: new UnthrottledAction());
            noThrottleCollection.PropertyChanged += propertyChangedHandler;
            for (int i = 0; i < 10000; i++)
            {
                noThrottleCollection.Add(i);
            }
            noThrottleCollection.PropertyChanged -= propertyChangedHandler;
            Assert.AreEqual(noThrottleCollection.Count, propertyChangedInvocations);

            // Throttling
            propertyChangedInvocations = 0;
            var throttleCollection = new ConcurrentObservableCollection<int>(controlledAction: new ThrottledActionTaskDelay(TimeSpan.FromMilliseconds(20)));
            throttleCollection.PropertyChanged += propertyChangedHandler;
            for (int i = 0; i < 10000; i++)
            {
                throttleCollection.Add(i);
            }
            throttleCollection.PropertyChanged -= propertyChangedHandler;
            Assert.IsTrue(propertyChangedInvocations < throttleCollection.Count);
        }

        /// <summary>
        /// When this was written the implementation of ThrottledAction
        /// had been changed. This tests the threadpool latency when
        /// using the new implementation. The latency will be written
        /// out to the console, not actually used in a test.
        /// </summary>
        [TestMethod]
        public void TestThreadPoolLatencyThrottledActionWithWait()
        {
            TestThreadLatency(()=>new ThrottledActionWithWait(TimeSpan.FromMilliseconds(20)));
        }

        /// <summary>
        /// When this was written the implementation of ThrottledAction
        /// had been changed. This tests the threadpool latency when
        /// using the old implementation. The latency will be written
        /// out to the console, not actually used in a test.
        /// </summary>
        [TestMethod]
        public void TestThreadPoolLatencyThrottledActionTaskDelay()
        {
            TestThreadLatency(()=>new ThrottledActionTaskDelay(TimeSpan.FromMilliseconds(20)));
        }

        /// <summary>
        /// When this was written the implementation of ThrottledAction
        /// had been changed. This tests the threadpool latency when
        /// using no throttling. The latency will be written
        /// out to the console, not actually used in a test.
        /// </summary>
        [TestMethod]
        public void TestThreadPoolLatencyUnthrottledAction()
        {
            TestThreadLatency(()=>new UnthrottledAction());
        }

        private void TestThreadLatency(Func<IControlledAction> actionFactory)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var startingLine = new ManualResetEvent(false);
            var threads = new List<Thread>();
            var threadCounter = new ConcurrentDictionary<int, int>();
            int eventCount = 0;
            int threadsStartedCount = 0;
            for (int i = 0; i < 100; ++i)
            {
                // Create a new thread instead of using the threadpool
                var thread = new Thread(new ThreadStart(() =>
                {
                    startingLine.WaitOne();
                    var token = cancellationTokenSource.Token;
                    int startValue = 0;
                    var testNew = new TestControlledActionLatency(actionFactory());
                    testNew.PropertyChanged += (s, e) =>
                    {
                        Interlocked.Increment(ref eventCount);
                        threadCounter[Thread.CurrentThread.ManagedThreadId] = 0;
                    };
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
            Thread.Sleep(2000);
            var resetEvent = new ManualResetEvent(false);
            var start = DateTime.Now;
            Task.Run(() =>
            {
                resetEvent.Set();
            });
            resetEvent.WaitOne();
            var duration = DateTime.Now - start;
            cancellationTokenSource.Cancel();
            Trace.WriteLine($"Task Run Latency = {duration}");
            Trace.WriteLine($"Events Fired = {eventCount}");
            Trace.WriteLine($"Threads Started = {threadsStartedCount}");
            Trace.WriteLine($"Threads Executed On = {threadCounter.Count}");
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
        public void TestCreatingThrottledActionWithWaitFromName()
        {
            string typeName = typeof(ThrottledActionWithWait).FullName;
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
            Assert.IsTrue(Activator.CreateInstance(type) is ThrottledActionWithWait, $"Couldn't create ThrottledAction from name {typeName}");
        }

        /// <summary>
        /// Testing creating a ThrottledAction from the name. Relevant to serialization,
        /// but more relevant to UnthrottledAction.
        /// </summary>
        [TestMethod]
        public void TestCreatingThrottledActionTaskDelayFromName()
        {
            string typeName = typeof(ThrottledActionTaskDelay).FullName;
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
            Assert.IsNotNull(type, $"Couldn't create ThrottleThrottledActionTaskDelaydAction from name {typeName}");
            Assert.IsTrue(Activator.CreateInstance(type) is ThrottledActionTaskDelay, $"Couldn't create ThrottledActionTaskDelay from name {typeName}");
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
