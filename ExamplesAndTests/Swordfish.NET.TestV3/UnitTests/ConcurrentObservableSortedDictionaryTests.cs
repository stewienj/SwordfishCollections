using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using Swordfish.NET.Collections.Auxiliary;
using Swordfish.NET.TestV3.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Swordfish.NET.TestV3.UnitTests
{
    [TestClass()]
    public class ConcurrentObservableSortedDictionaryTests
    {

        [TestMethod()]
        public void AddTest()
        {
            int _testCollectionCount = 10;
            int _itemsPerCollection = 200_000;
            List<List<int>> _testCollections = new List<List<int>>();
            List<int> _sortedCollection = new List<int>();

            // Use a fixed seed for consistency in results
            Random random = new Random(1);

            // Create 10 test sets
            for (int collection = 0; collection < _testCollectionCount; ++collection)
            {
                List<int> testCollection = new List<int>();
                for (int item = 0; item < _itemsPerCollection; ++item)
                {
                    // Ensure we have some duplicates by picking a random number
                    // less than half the number of items.
                    testCollection.Add(random.Next(_itemsPerCollection / 2));
                }
                _testCollections.Add(testCollection);
            }

            _sortedCollection = _testCollections
                .SelectMany(x => x)
                .Distinct()
                .OrderBy(x => x)
                .ToList();


            ConcurrentObservableSortedDictionary<int, int> subject = new ConcurrentObservableSortedDictionary<int, int>();
            using (var benchmark = new BenchmarkIt("Adding items to sorted dictionary"))
            {
                // Create test subject
                // Populate test subject
                _testCollections.AsParallel().ForAll(collection =>
                {
                    foreach (var item in collection)
                    {
                        subject[item] = item;
                    }
                });
            }

            bool keyMatchesValue = subject
                .All(kv => kv.Key == kv.Value);

            Assert.IsTrue(keyMatchesValue);

            bool isSorted = subject
                .Aggregate(
                    new { Sorted = true, LastKey = (int?)null },
                    (a, b) => new { Sorted = a.Sorted && (!a.LastKey.HasValue || a.LastKey.Value < b.Key), LastKey = (int?)b.Key })
                .Sorted;

            Assert.IsTrue(isSorted);

            // Compare test subject with expected result
            Assert.AreEqual(subject.Count, _sortedCollection.Count);
            bool itemsEqual = _sortedCollection
              .Zip(subject, (a, b) => a == b.Value)
              .All(b => b);
            Assert.IsTrue(itemsEqual);

            // Compare collectionView
            var view = subject.CollectionView;
            Assert.AreEqual(view.Count, _sortedCollection.Count);
            bool viewItemsEqual = _sortedCollection
              .Zip(view, (a, b) => a == b.Value)
              .All(b => b);
            Assert.IsTrue(viewItemsEqual);
        }


        [TestMethod]
        public void TestManyOperations()
        {
            // Create some random, but unique items
            // Use a fixed seed for consistency in results
            Random random = new Random(1);
            HashSet<int> baseItemsSet = new HashSet<int>();
            while (baseItemsSet.Count < 1_100_000)
            {
                baseItemsSet.Add(random.Next());
            }

            // Create 2 collections, 1 to test, and 1 to compare against
            var testCollection = new ConcurrentObservableSortedDictionary<string, string>();
            var sortedDictionary = new SortedDictionary<string, string>();

            // Create 1,000,000 items to add and insert
            var itemsToAdd =
                baseItemsSet
                .Take(1_000_000)
                .Select(x => KeyValuePair.Create($"Key {x}", $"Value {x}"))
                .ToList();

            // Create 100,000 items to insert
            var itemsToInsert =
                baseItemsSet
                .Skip(1_000_000)
                .Take(100_000)
                .Select(x => KeyValuePair.Create($"Insert Key {x}", $"Insert Value {x}"))
                .ToList();

            // Create items to remove
            var itemsToRemove =
                itemsToInsert
                .Take(1000)
                .ToList();

            foreach (var item in itemsToAdd)
            {
                sortedDictionary.Add(item.Key, item.Value);
                testCollection.Add(item.Key, item.Value);
            }

            // Check items are equal count
            Assert.IsTrue(sortedDictionary.Count == testCollection.Count, "Added Items correct count");

            // Check items are equal order
            var allEqualAfterAdd =
                sortedDictionary
                .Zip(testCollection, (a, b) => (a.Key == b.Key) && (a.Value == b.Value))
                .All(a => a);

            Assert.IsTrue(allEqualAfterAdd, "Added items correct order");


            // Test inserting items

            int insertIndex = itemsToInsert.Count + 100;
            foreach (var item in itemsToInsert)
            {
                // Naturally sorted dictionary doesn't support insert at
                sortedDictionary.Add(item.Key, item.Value);
                // We have the function but it's there for other reasons
                testCollection.Insert(insertIndex, item);

                insertIndex--;
            }

            // Check items are equal count
            Assert.IsTrue(sortedDictionary.Count == testCollection.Count, "Items correct count after inserting");

            // Check items are equal order
            var allEqualAfterInsert =
                sortedDictionary
                .Zip(testCollection, (a, b) => (a.Key == b.Key) && (a.Value == b.Value))
                .All(a => a);

            Assert.IsTrue(allEqualAfterAdd, "Items correct order after insert");

            // Test removing items

            foreach (var item in itemsToRemove)
            {
                sortedDictionary.Remove(item.Key);
                testCollection.Remove(item.Key);
            }

            // Check items are equal count
            Assert.IsTrue(sortedDictionary.Count == testCollection.Count, "Items correct count after removing");

            // Check items are equal order
            var allEqualAfterRemove =
                sortedDictionary
                .Zip(testCollection, (a, b) => (a.Key == b.Key) && (a.Value == b.Value))
                .All(a => a);

            Assert.IsTrue(allEqualAfterRemove, "Items correct order after removing");

            // Test contains

            var containsAll = sortedDictionary
                .All(kv => testCollection.Contains(kv));

            Assert.IsTrue(containsAll, "Contains all the items is true");

            var containsNone = itemsToRemove
                .Any(kv => testCollection.ContainsKey(kv.Key));

            Assert.IsFalse(containsNone, "Contains any of the removed items is false");


            // Test removing at

            var sortedList = new SortedList<string, string>(sortedDictionary);

            int removeAtIndex = sortedDictionary.Count - 30;
            while (removeAtIndex >= 0 && sortedDictionary.Count > 0)
            {
                sortedList.RemoveAt(removeAtIndex);
                testCollection.RemoveAt(removeAtIndex);
                removeAtIndex -= 30;
            }

            // Check items are equal count
            Assert.IsTrue(sortedList.Count == testCollection.Count, "Items correct count after removing at index");

            // Check items are equal order
            var allEqualAfterRemoveAt =
                sortedList
                .Zip(testCollection, (a, b) => (a.Key == b.Key) && (a.Value == b.Value))
                .All(a => a);

            Assert.IsTrue(allEqualAfterRemoveAt, "Items correct order after removing at index");

            var list =
                sortedList
                .Select(x=>x.Key)
                .ToList();

            bool getItemCorrect = true;

            for(int i=0; i<list.Count; ++i)
            {
                getItemCorrect &= testCollection.GetItem(i).Key == list[i];
            }

            Assert.IsTrue(getItemCorrect, "Get item by index correct");

            /*

            [TestMethod()]
            public void AddRangeTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void RemoveTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void RemoveRangeTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void ToStringTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void GetEnumeratorTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void ClearTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void ContainsTest()
            {
              Assert.Fail();
            }

            [TestMethod()]
            public void CopyToTest()
            {
              Assert.Fail();
            }
            */
        }
    }
}