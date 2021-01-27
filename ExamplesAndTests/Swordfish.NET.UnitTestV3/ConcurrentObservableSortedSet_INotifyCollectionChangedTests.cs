using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Swordfish.NET.UnitTestV3
{
    /// <summary>
    /// Tests that the following methods fire the correct event in ConcurrentObservableSortedSet:
    ///
    /// - AddRange
    /// - InsertRange
    /// - RemoveRange
    /// - Reset
    /// - Clear
    ///
    /// Test the following collection classes:
    ///
    /// - ConcurrentObservableCollection - done (this class)
    /// - ConcurrentObservableDictionary - done (other class)
    /// - ConcurrentObservableHashSet
    /// - ConcurrentObservableSortedCollection
    /// - ConcurrentObservableSortedDictionary
    /// - ConcurrentObservableSortedSet
    /// </summary>
    [TestClass]
    public class ConcurrentObservableSortedSet_INotifyCollectionChangedTests
    {
        [TestMethod]
        public void Test_ConcurrentObservableSortedSet_Clear()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var collection = new ConcurrentObservableSortedSet<int>();
            foreach (var item in initial)
            {
                collection.Add(item);
            }
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.Clear();

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.IsNotNull(returnedObject);
            Assert.IsNotNull(returnedArgs);

            Assert.AreEqual(0, collection.Count);

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, returnedArgs.Action);

            Assert.IsNull(returnedArgs.NewItems);

            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(initial.Count(), returnedArgs.OldItems.Count);
            Assert.IsTrue(CollectionsAreEqual(initial, returnedArgs.OldItems));
        }

        bool CollectionsAreEqual(IEnumerable<int> collectionA, IList collectionB) =>
            collectionA.Zip(collectionB.OfType<int>(), (a, b) => a == b).All(c => c);
    }
}
