using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
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
    /// - ConcurrentObservableCollection - done (this class)
    /// - ConcurrentObservableDictionary
    /// - ConcurrentObservableHashSet
    /// - ConcurrentObservableSortedCollection
    /// - ConcurrentObservableSortedDictionary
    /// - ConcurrentObservableSortedSet
    /// </summary>
    [TestClass]
    public class ConcurrentObservableCollection_INotifyCollectionChangedTests
    {
        [TestMethod]
        public void Test_ConcurrentObservableCollection_AddRange()
        {
            var toAdd = Enumerable.Range(0, 100).ToList();
            var collection = new ConcurrentObservableCollection<int>();

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
            Assert.IsTrue(toAdd.Zip(returnedArgs.NewItems.OfType<int>(), (a, b) => a == b).All(c => c));

        }

        [TestMethod]
        public void Test_ConcurrentObservableCollection_InsertRange()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var toAdd = Enumerable.Range(100, 100).ToList();
            var startIndex = 50;
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(initial);
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.InsertRange(startIndex, toAdd);

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, returnedArgs.Action);
            Assert.AreEqual(startIndex, returnedArgs.NewStartingIndex);
            Assert.IsNotNull(returnedArgs.NewItems);
            Assert.IsNull(returnedArgs.OldItems);
            Assert.AreEqual(toAdd.Count(), returnedArgs.NewItems.Count);
            Assert.IsTrue(toAdd.Zip(returnedArgs.NewItems.OfType<int>(), (a, b) => a == b).All(c => c));
        }

        [TestMethod]
        public void Test_ConcurrentObservableCollection_RemoveRange()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var startIndex = 50;
            var removeCount = 40;
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(initial);
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.RemoveRange(startIndex, removeCount);

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.IsNotNull(returnedObject);
            Assert.IsNotNull(returnedArgs);

            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, returnedArgs.Action);
            Assert.AreEqual(startIndex, returnedArgs.OldStartingIndex);
            Assert.IsNull(returnedArgs.NewItems);
            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(removeCount, returnedArgs.OldItems.Count);
        }

        [TestMethod]
        public void Test_ConcurrentObservableCollection_Reset()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var toAdd = Enumerable.Range(100, 100).ToList();
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(initial);
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.Reset(toAdd);

            // Check two collection changed events were fired
            Assert.AreEqual(2, returnedList.Count);
            (var returnedObject0, var returnedArgs0) = returnedList[0];
            (var returnedObject1, var returnedArgs1) = returnedList[1];

            Assert.IsNotNull(returnedObject0);
            Assert.IsNotNull(returnedArgs0);
            Assert.IsNotNull(returnedObject1);
            Assert.IsNotNull(returnedArgs1);

            Assert.AreEqual(returnedObject0, collection);
            Assert.AreEqual(returnedObject1, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, returnedArgs0.Action);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, returnedArgs1.Action);

            Assert.IsNull(returnedArgs0.NewItems);
            Assert.IsNotNull(returnedArgs0.OldItems);
            Assert.AreEqual(initial.Count(), returnedArgs0.OldItems.Count);
            Assert.IsTrue(initial.Zip(returnedArgs0.OldItems.OfType<int>(), (a, b) => a == b).All(c => c));

            Assert.IsNull(returnedArgs1.OldItems);
            Assert.IsNotNull(returnedArgs1.NewItems);
            Assert.AreEqual(toAdd.Count(), returnedArgs1.NewItems.Count);
            Assert.IsTrue(toAdd.Zip(returnedArgs1.NewItems.OfType<int>(), (a, b) => a == b).All(c => c));
        }

        [TestMethod]
        public void Test_ConcurrentObservableCollection_Clear()
        {
            var initial = Enumerable.Range(0, 100).ToList();
            var collection = new ConcurrentObservableCollection<int>();
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
            Assert.IsTrue(initial.Zip(returnedArgs.OldItems.OfType<int>(), (a, b) => a == b).All(c => c));
        }

    }
}
