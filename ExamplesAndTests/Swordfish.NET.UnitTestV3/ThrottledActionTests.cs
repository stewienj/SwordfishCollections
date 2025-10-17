using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System.Linq;
using Swordfish.NET.Collections.Auxiliary;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Threading;

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

    }
}
