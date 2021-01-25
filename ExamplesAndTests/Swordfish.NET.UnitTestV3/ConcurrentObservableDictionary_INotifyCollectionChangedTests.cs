using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;

namespace Swordfish.NET.UnitTestV3
{
    /// <summary>
    /// Tests that the following methods fire the correct event in ConcurrentObservableDictionary:
    ///
    /// - AddRange
    /// - RemoveRange
    /// - Clear
    ///
    /// Test the following collection classes:
    ///
    /// - ConcurrentObservableCollection - done (other class)
    /// - ConcurrentObservableDictionary
    /// - ConcurrentObservableHashSet
    /// - ConcurrentObservableSortedCollection
    /// - ConcurrentObservableSortedDictionary
    /// - ConcurrentObservableSortedSet
    /// </summary>
    [TestClass]
    public class ConcurrentObservableDictionary_INotifyCollectionChangedTests
    {
        [TestMethod]
        public void Test_ConcurrentObservableDictionary_AddRange_IEnumerable()
        {
            var toAdd = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var collection = new ConcurrentObservableDictionary<int, int>();

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
            Assert.IsTrue(toAdd.Zip(returnedArgs.NewItems.OfType<KeyValuePair<int, int>>(), (a, b) => a.Key == b.Key && a.Value == b.Value).All(c => c));
        }

        [TestMethod]
        public void Test_ConcurrentObservableDictionary_AddRange_List()
        {
            var toAdd = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x)).ToList();
            var collection = new ConcurrentObservableDictionary<int, int>();

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
            Assert.IsTrue(toAdd.Zip(returnedArgs.NewItems.OfType<KeyValuePair<int, int>>(), (a, b) => a.Key == b.Key && a.Value == b.Value).All(c => c));
        }

        [TestMethod]
        public void Test_ConcurrentObservableDictionary_AddRange_Dictionary()
        {
            var toAdd = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x)).ToDictionary(x => x.Key, x => x.Value);
            var collection = new ConcurrentObservableDictionary<int, int>();

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
            Assert.IsTrue(toAdd.Zip(returnedArgs.NewItems.OfType<KeyValuePair<int, int>>(), (a, b) => a.Key == b.Key && a.Value == b.Value).All(c => c));
        }

        [TestMethod]
        public void Test_ConcurrentObservableCollection_RemoveRange_ByIndex()
        {
            var initial = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var startIndex = 50;
            var removeCount = 40;
            var collection = new ConcurrentObservableDictionary<int, int>();
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
        public void Test_ConcurrentObservableDictionary_Clear()
        {
            var initial = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var collection = new ConcurrentObservableDictionary<int, int>();
            collection.AddRange(initial);
            Assert.AreEqual(100, collection.Count);

            // Record all the collection changed events
            List<(object, NotifyCollectionChangedEventArgs)> returnedList = new List<(object, NotifyCollectionChangedEventArgs)>();
            collection.CollectionChanged += (s, e) => returnedList.Add((s, e));

            collection.Clear();

            // Check just one collection changed event was fired
            Assert.AreEqual(1, returnedList.Count);
            (var returnedObject, var returnedArgs) = returnedList[0];

            Assert.AreEqual(0, collection.Count);
            
            Assert.AreEqual(returnedObject, collection);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, returnedArgs.Action);

            Assert.IsNull(returnedArgs.NewItems);

            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(initial.Count(), returnedArgs.OldItems.Count);
            Assert.IsTrue(initial.Zip(returnedArgs.OldItems.OfType<KeyValuePair<int, int>>(), (a, b) => a.Key == b.Key && a.Value == b.Value).All(c => c));
            
        }
    }
}
