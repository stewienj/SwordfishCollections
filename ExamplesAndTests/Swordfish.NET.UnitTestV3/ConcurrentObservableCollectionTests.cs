using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class ConcurrentObservableCollectionTests
    {
        [TestMethod]
        public void ResetNotifyTest()
        {
            // Check that the CollectionView property changed event occurs when we do a reset

            ConcurrentObservableSortedCollection<int> subject = new ConcurrentObservableSortedCollection<int>();

            AutoResetEvent autoReset = new AutoResetEvent(false);

            subject.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(subject.CollectionView))
                {
                    autoReset.Set();
                }
            };

            // Ensure the CollectionView property changed event hasn't been fired yet
            Assert.IsFalse(autoReset.WaitOne(1000));

            // Do a reset and hopefully fire the CollectionView property changed
            subject.Reset(Enumerable.Range(0, 100).ToList());

            // Ensure the CollectionView property changed event has been fired
            Assert.IsTrue(autoReset.WaitOne(1000));

            // Do a repeat just for good luck

            // Ensure the CollectionView property changed event hasn't been fired yet
            Assert.IsFalse(autoReset.WaitOne(1000));

            // Do a reset and hopefully fire the CollectionView property changed
            subject.Reset(Enumerable.Range(0, 100).ToList());

            // Ensure the CollectionView property changed event has been fired
            Assert.IsTrue(autoReset.WaitOne(1000));
        }
    }
}
