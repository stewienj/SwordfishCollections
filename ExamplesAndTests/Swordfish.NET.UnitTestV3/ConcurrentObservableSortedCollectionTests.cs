using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using Swordfish.NET.UnitTestV3.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class ConcurrentObservableSortedCollectionTests
    {
        private int _testCollectionCount = 10;
        private int _itemsPerCollection = 100_000;
        private List<List<int>> _testCollections = new List<List<int>>();
        private List<int> _sortedCollection = new List<int>();

        public ConcurrentObservableSortedCollectionTests()
        {
            GenerateTestCollections();
        }

        private int TotalItems => _testCollectionCount * _itemsPerCollection;

        private bool IsSorted(IEnumerable<int> list)
        {
            int previous = list.First();
            foreach (var current in list.Skip(1))
            {
                if (previous > current)
                {
                    return false;
                }
                previous = current;
            }
            return true;
        }

        private void GenerateTestCollections()
        {
            using (var benchmark = new BenchmarkIt("Generating Test Input Collections for sorted collection"))
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

            using (var benchmark = new BenchmarkIt("Generating Expected Output for sorted collection"))
            {
                _sortedCollection = _testCollections
                  .SelectMany(x => x)
                  .OrderBy(x => x)
                  .ToList();
            }

        }

        [TestMethod]
        public void AddTest()
        {
            ConcurrentObservableSortedCollection<int> subject = new ConcurrentObservableSortedCollection<int>();
            using (var benchmark = new BenchmarkIt("Adding items to sorted collection"))
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
            Assert.AreEqual(subject.Count, _sortedCollection.Count);
            bool itemsEqual = _sortedCollection
              .Zip(subject, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(itemsEqual);

            // Compare collectionView
            var view = subject.CollectionView;
            Assert.AreEqual(view.Count, _sortedCollection.Count);
            bool viewItemsEqual = _sortedCollection
              .Zip(view, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(viewItemsEqual);
        }

        [TestMethod]
        public void AddRangeTest()
        {
            ConcurrentObservableSortedCollection<int> subject = new ConcurrentObservableSortedCollection<int>();
            using (var benchmark = new BenchmarkIt("Adding items to sorted collection"))
            {
                // Create test subject
                // Populate test subject
                _testCollections.AsParallel().ForAll(collection =>
                {
                    subject.AddRange(collection);
                });
            }
            // Compare test subject with expected result
            Assert.AreEqual(subject.Count, _sortedCollection.Count);
            bool itemsEqual = _sortedCollection
              .Zip(subject, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(itemsEqual);

            // Compare collectionView
            var view = subject.CollectionView;
            Assert.AreEqual(view.Count, _sortedCollection.Count);
            bool viewItemsEqual = _sortedCollection
              .Zip(view, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(viewItemsEqual);
        }


        [TestMethod]
        public void ResetTest()
        {
            ConcurrentObservableSortedCollection<int> subject = new ConcurrentObservableSortedCollection<int>();
            // Use a fixed seed for consistency in results
            Random random = new Random(2);
            for (int item = 0; item < _itemsPerCollection; ++item)
            {
                // Ensure we have some duplicates by picking a random number
                // less than half the number of items.
                subject.Add(random.Next(_itemsPerCollection / 2));
            }

            subject.Reset(_testCollections.SelectMany(x => x).ToList());

            // Compare test subject with expected result
            Assert.AreEqual(subject.Count, _sortedCollection.Count);
            bool itemsEqual = _sortedCollection
              .Zip(subject, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(itemsEqual);

            // Compare collectionView
            var view = subject.CollectionView;
            Assert.AreEqual(view.Count, _sortedCollection.Count);
            bool viewItemsEqual = _sortedCollection
              .Zip(view, (a, b) => a == b)
              .All(b => b);
            Assert.IsTrue(viewItemsEqual);
        }

        [TestMethod]
        public void IndexTest()
        {
            // This test populates the subject with one collection, and then overwrites it with another collection
            // however as the subject gets sorted on write the 2nd collection won't be equal to the subject at the
            // end, all we can test for is that it is sorted
            ConcurrentObservableSortedCollection<int> subject = new ConcurrentObservableSortedCollection<int>();

            // Get test collections
            var sourceCollection1 = _testCollections[0];
            var sourceCollection2 = _testCollections[1];

            // Check the source collections aren't sorted
            Assert.IsFalse(IsSorted(sourceCollection1));
            Assert.IsFalse(IsSorted(sourceCollection2));

            // Check the source collections are different
            bool sourceCollectionsEqual = sourceCollection1
              .Zip(sourceCollection2, (a, b) => a == b)
              .All(b => b);
            Assert.IsFalse(sourceCollectionsEqual);

            // Populate subject
            subject.AddRange(sourceCollection1);
            Assert.IsTrue(IsSorted(subject));

            // Check the counts are all ok
            Assert.AreEqual(sourceCollection1.Count, subject.Count);
            Assert.AreEqual(sourceCollection2.Count, subject.Count);
            int count = subject.Count;

            // Overwrite items from the second collection, checking counts all the way
            for(int i=0; i<count; ++i)
            {
                subject[i] = sourceCollection2[i];
                Assert.AreEqual(count, subject.Count);
            }
            Assert.IsTrue(IsSorted(subject));

            // Now try overwriting with a sorted collection where all
            // the values are less than what's currently in the collection.
            // The resulting collection should be the same as the sorted
            // source collection
            var sourceCollection2Sorted = sourceCollection2.Select(x=>x- _itemsPerCollection).OrderBy(x => x).ToList();
            for (int i = 0; i < count; ++i)
            {
                subject[i] = sourceCollection2Sorted[i];
                Assert.AreEqual(count, subject.Count);
            }
            Assert.IsTrue(IsSorted(subject));

            bool viewItemsEqual = sourceCollection2Sorted
                .Zip(subject, (a, b) => a == b)
                .All(b => b);
            Assert.IsTrue(viewItemsEqual);
        }


    }
}
