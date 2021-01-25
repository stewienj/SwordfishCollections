using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using Swordfish.NET.UnitTestV3.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass()]
    public class ConcurrentObservableSortedSetTests
    {
        private int _testCollectionCount = 10;
        private int _itemsPerCollection = 1_000_000;
        private List<List<int>> _testCollections = new List<List<int>>();
        private List<int> _sortedSet = new List<int>();

        public ConcurrentObservableSortedSetTests()
        {
            GenerateTestCollections();
            GenerateSortedSet();
            TimeNormalSortedSet();
            TimeImmutableSortedSet();
        }

        private void GenerateTestCollections()
        {
            using (var benchmark = new BenchmarkIt("Generating Test Input Collections for sorted set"))
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

        private void TimeNormalSortedSet()
        {
            var sortedSet = new SortedSet<int>();
            using (var benchmark = new BenchmarkIt("Timing adding everything to normal sorted set"))
            {
                foreach (var collection in _testCollections)
                {
                    foreach (var item in collection)
                    {
                        sortedSet.Add(item);
                    }
                }
            }
        }

        private void TimeImmutableSortedSet()
        {
            var sortedSet = ImmutableSortedSet<int>.Empty;
            using (var benchmark = new BenchmarkIt("Timing adding everything to an immutable sorted set"))
            {
                foreach (var collection in _testCollections)
                {
                    foreach (var item in collection)
                    {
                        sortedSet = sortedSet.Add(item);
                    }
                }
            }
        }

        private void GenerateSortedSet()
        {
            using (var benchmark = new BenchmarkIt("Generating Expected Output for sorted set"))
            {
                _sortedSet = _testCollections
                  .SelectMany(x => x)
                  .Distinct()
                  .OrderBy(x => x)
                  .ToList();
            }
        }

        [TestMethod()]
        public void AddTest()
        {
            ConcurrentObservableSortedSet<int> subject = new ConcurrentObservableSortedSet<int>();
            using (var benchmark = new BenchmarkIt("Adding items to sorted set"))
            {
                // Create test subject
                // Populate test subject
                _testCollections.AsParallel().ForAll(collection =>
                {
                    foreach (var item in collection)
                    {
                        subject.Add(item);
                    }
                });
            }
            // Compare test subject with expected result
            Assert.AreEqual(subject.Count, _sortedSet.Count);
            bool itemsEqual = _sortedSet
              .Zip(subject, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(itemsEqual);

            // Compare collectionView
            var view = subject.CollectionView;
            Assert.AreEqual(view.Count, _sortedSet.Count);
            bool viewItemsEqual = _sortedSet
              .Zip(view, (a, b) => a == b)
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
