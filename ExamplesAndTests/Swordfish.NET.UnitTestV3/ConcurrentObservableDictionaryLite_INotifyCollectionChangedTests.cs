using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;

namespace Swordfish.NET.UnitTestV3
{
    /// <summary>
    /// Tests that the following methods fire the correct event in ConcurrentObservableDictionaryLite:
    ///
    /// - AddRange
    /// - RemoveRange
    /// - Clear
    ///
    /// Test the following collection classes:
    ///
    /// - ConcurrentObservableCollection - done (other class)
    /// - ConcurrentObservableDictionaryLite - done (this class)
    /// - ConcurrentObservableHashSet
    /// - ConcurrentObservableSortedCollection
    /// - ConcurrentObservableSortedDictionary
    /// - ConcurrentObservableSortedSet
    /// </summary>
    [TestClass]
    public class ConcurrentObservableDictionaryLite_INotifyCollectionChangedTests
    {
        [TestMethod]
        public void Test_ConcurrentObservableDictionaryLite_AddRange_IEnumerable()
        {
            var toAdd = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var collection = new ConcurrentObservableDictionaryLite<int, int>();

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
        public void Test_ConcurrentObservableDictionaryLite_AddRange_List()
        {
            var toAdd = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x)).ToList();
            var collection = new ConcurrentObservableDictionaryLite<int, int>();

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
        public void Test_ConcurrentObservableDictionaryLite_AddRange_Dictionary()
        {
            var toAdd = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x)).ToDictionary(x => x.Key, x => x.Value);
            var collection = new ConcurrentObservableDictionaryLite<int, int>();

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
        public void Test_ConcurrentObservableCollection_RemoveRange_ByItems_IList()
        {
            var initial = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var toRemove = Enumerable.Range(1, 40).Select(x => x * 2).ToList();
            var collection = new ConcurrentObservableDictionaryLite<int, int>();
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
            Assert.AreEqual(-1, returnedArgs.OldStartingIndex);
            Assert.IsNull(returnedArgs.NewItems);
            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(toRemove.Count, returnedArgs.OldItems.Count);
            Assert.IsTrue(CollectionsAreEqual(toRemove, returnedArgs.OldItems));
        }

        [TestMethod]
        public void Test_ConcurrentObservableCollection_RemoveRange_ByItems_IEnumerable()
        {
            var initial = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var toRemove = Enumerable.Range(1, 40).Select(x => x * 2);
            var collection = new ConcurrentObservableDictionaryLite<int, int>();
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
            Assert.AreEqual(-1, returnedArgs.OldStartingIndex);
            Assert.IsNull(returnedArgs.NewItems);
            Assert.IsNotNull(returnedArgs.OldItems);
            Assert.AreEqual(toRemove.Count(), returnedArgs.OldItems.Count);
            Assert.IsTrue(CollectionsAreEqual(toRemove, returnedArgs.OldItems));
        }


        [TestMethod]
        public void Test_ConcurrentObservableDictionaryLite_Clear()
        {
            var initial = Enumerable.Range(0, 100).Select(x => new KeyValuePair<int, int>(x, x));
            var collection = new ConcurrentObservableDictionaryLite<int, int>();
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

        bool CollectionsAreEqual(IEnumerable<KeyValuePair<int, int>> collectionA, IList collectionB) =>
            collectionA.Zip(collectionB.OfType<KeyValuePair<int, int>>(), (a, b) => a.Key == b.Key && a.Value == b.Value).All(c => c);

        bool CollectionsAreEqual(IEnumerable<int> collectionA, IList collectionB) =>
            collectionA.Zip(collectionB.OfType<int>(), (a, b) => a == b).All(c => c);

        [TestMethod]
        public void Test_ConcurrentObservableDictionaryLite_With_EqualityComparer()
        {
            BoxEqualityComparer boxEqC = new BoxEqualityComparer();

            var boxes = new Dictionary<Box, string>(boxEqC);

            var redBox = new Box(4, 3, 4);
            boxes.Add(redBox, "red");

            var blueBox = new Box(4, 3, 4);
            Assert.ThrowsException<ArgumentException>(
                action: () => boxes.Add(blueBox, "blue"),
                message: "An item with the same key has already been added. Key: (4, 3, 4)");

            var greenBox = new Box(3, 4, 3);
            boxes.Add(greenBox, "green");
            Console.WriteLine();

            Assert.AreEqual(2, boxes.Count);
        }

        public class Box
        {
            public Box(int h, int l, int w)
            {
                this.Height = h;
                this.Length = l;
                this.Width = w;
            }

            public int Height { get; set; }
            public int Length { get; set; }
            public int Width { get; set; }

            public override String ToString()
            {
                return String.Format("({0}, {1}, {2})", Height, Length, Width);
            }
        }

        class BoxEqualityComparer : IEqualityComparer<Box>
        {
            public bool Equals(Box b1, Box b2)
            {
                if (b2 == null && b1 == null)
                    return true;
                else if (b1 == null || b2 == null)
                    return false;
                else if (b1.Height == b2.Height && b1.Length == b2.Length
                                    && b1.Width == b2.Width)
                    return true;
                else
                    return false;
            }

            public int GetHashCode(Box bx)
            {
                int hCode = bx.Height ^ bx.Length ^ bx.Width;
                return hCode.GetHashCode();
            }
        }
    }
}
