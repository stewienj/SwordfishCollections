using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
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
        private int _testCollectionCount = 10;
        private int _itemsPerCollection = 100_000;
        private List<List<int>> _testCollections = new List<List<int>>();
        private List<int> _sortedCollection = new List<int>();

        public ConcurrentObservableSortedDictionaryTests()
        {
            GenerateTestCollections();
            GenerateSortedCollection();
            TimeNormalSortedDictionary();
            TimeImmutableSortedDictionary();
        }

        private void GenerateTestCollections()
        {
            using (var benchmark = new BenchmarkIt("Generating Test Input Collections for sorted dictionary"))
            {
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
            }
        }

        private void TimeNormalSortedDictionary()
        {
            var sortedDictionary = new SortedDictionary<int,int>();
            using (var benchmark = new BenchmarkIt("Timing adding everything to normal sorted dictionary"))
            {
                foreach (var collection in _testCollections)
                {
                    foreach (var item in collection)
                    {
                        sortedDictionary[item] = item;
                    }
                }
            }
        }

        private void TimeImmutableSortedDictionary()
        {
            var sortedDictionary = ImmutableSortedDictionary<int,int>.Empty;
            using (var benchmark = new BenchmarkIt("Timing adding everything to an immutable sorted dictionary"))
            {
                foreach (var collection in _testCollections)
                {
                    foreach (var item in collection)
                    {
                        sortedDictionary = sortedDictionary.SetItem(item,item);
                    }
                }
            }
        }

        private void GenerateSortedCollection()
        {
            using (var benchmark = new BenchmarkIt("Generating Expected Output for sorted dictionary"))
            {
                _sortedCollection = _testCollections
                  .SelectMany(x => x)
                  .Distinct()
                  .OrderBy(x => x)
                  .ToList();
            }
        }

        [TestMethod()]
        public void AddTest()
        {
            ConcurrentObservableSortedDictionary<int,int> subject = new ConcurrentObservableSortedDictionary<int,int>();
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