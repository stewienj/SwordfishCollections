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
    /// Tests that the following methods fire the correct event in ConcurrentObservableCollection:
    ///
    /// - AddRange
    /// - InsertRange
    /// - RemoveRange
    /// - Reset
    /// - Clear
    ///
    /// Test the following collection classes:
    ///
    /// - ConcurrentObservableCollection - done (other class)
    /// - ConcurrentObservableDictionary - done (other class)
    /// - ConcurrentObservableHashSet - done (this class)
    /// - ConcurrentObservableSortedCollection
    /// - ConcurrentObservableSortedDictionary
    /// - ConcurrentObservableSortedSet
    /// </summary>
    [TestClass]
    public class ConcurrentObservableHashSet_INotifyCollectionChangedTests
    {
        [TestMethod]
        public void Test_ConcurrentObservableHashSet_AddRange_IEnumerable()
        {
            var toAdd = Enumerable.Range(0, 100);
            var collection = new ConcurrentObservableHashSet<int>();

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.AddRange(toAdd);

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(returnedArgs.Action, NotifyCollectionChangedAction.Add);
            Assert.IsNotNull(returnedArgs.NewItems);
            Assert.IsNull(returnedArgs.OldItems);
            Assert.AreEqual(toAdd.Count(), returnedArgs.NewItems.Count);
            Assert.IsTrue(CollectionsAreEqual(toAdd, returnedArgs.NewItems));
        }

        [TestMethod]
        public void Test_ConcurrentObservableHashSet_AddRange_IList()
        {
            var toAdd = Enumerable.Range(0, 100).ToList();
            var collection = new ConcurrentObservableHashSet<int>();

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.AddRange(toAdd);

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(returnedArgs.Action, NotifyCollectionChangedAction.Add);
            Assert.IsNotNull(returnedArgs.NewItems);
            Assert.IsNull(returnedArgs.OldItems);
            Assert.AreEqual(toAdd.Count(), returnedArgs.NewItems.Count);
            Assert.IsTrue(CollectionsAreEqual(toAdd, returnedArgs.NewItems));
        }

        [TestMethod]
        public void Test_ConcurrentObservableHashSet_RemoveRange_IEnumerable()
        {
            var initial = Enumerable.Range(0, 100);
            var startIndex = 50;
            var removeCount = 40;
            var toRemove = initial.Skip(startIndex).Take(removeCount).ToList();
            var collection = new ConcurrentObservableHashSet<int>();
            collection.AddRange(initial);
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.RemoveRange(toRemove);

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.IsNotNull(returnedObject);
            Assert.IsNotNull(returnedArgs);

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, returnedArgs.Action);
            // Removed by values so index not relevant, should be -1
            Assert.AreEqual(-1, returnedArgs.OldStartingIndex);
            Assert.IsNull(returnedArgs.NewItems);
            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(removeCount, returnedArgs.OldItems.Count);
            Assert.IsTrue(CollectionsAreEqual(toRemove, returnedArgs.OldItems));
        }

        [TestMethod]
        public void Test_ConcurrentObservableHashSet_RemoveRange_IList()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var startIndex = 50;
            var removeCount = 40;
            var toRemove = initial.Skip(startIndex).Take(removeCount).ToList();
            var collection = new ConcurrentObservableHashSet<int>();
            collection.AddRange(initial);
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.RemoveRange(toRemove);

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.IsNotNull(returnedObject);
            Assert.IsNotNull(returnedArgs);

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, returnedArgs.Action);
            // Removed by values so index not relevant, should be -1
            Assert.AreEqual(-1, returnedArgs.OldStartingIndex);
            Assert.IsNull(returnedArgs.NewItems);
            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(removeCount, returnedArgs.OldItems.Count);
            Assert.IsTrue(CollectionsAreEqual(toRemove, returnedArgs.OldItems));
        }

        [TestMethod]
        public void Test_ConcurrentObservableHashSet_Clear()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var collection = new ConcurrentObservableHashSet<int>();
            collection.AddRange(initial);
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
