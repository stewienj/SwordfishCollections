using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Swordfish.NET.UnitTestV3
{
    /// <summary>
    /// This is a group of tests for Issue 35 on github
    /// https://github.com/stewienj/SwordfishCollections/issues/35
    /// </summary>
    [TestClass]
    public class ConcurrentObservableDictionary_Issue35
    {
        [TestMethod]
        public void AddRangeRemoveRangeTest()
        {
            var keyValuePairs = Enumerable.Range(0, 10)
                .Select(i => new KeyValuePair<object, int>(new object(), i)).ToArray();

            var testCollection = new ConcurrentObservableDictionary<object, int>();
            testCollection.AddRange(keyValuePairs);

            // Test collections are equivalent
            CollectionAssert.AreEquivalent(keyValuePairs, testCollection);

            testCollection.RemoveRange(keyValuePairs.Take(5).Select(pair => pair.Key));

            // Test collections are equivalent
            Assert.AreEqual(5, testCollection.Count);
            CollectionAssert.AreEquivalent(keyValuePairs.Skip(5).ToList(), testCollection);
        }

        [TestMethod]
        public void AddRangeRemoveTest()
        {
            var keyValuePairs = Enumerable.Range(0, 10)
                .Select(i => new KeyValuePair<object, int>(new object(), i)).ToArray();

            var testCollection = new ConcurrentObservableDictionary<object, int>();
            testCollection.AddRange(keyValuePairs);

            // Test collections are equivalent
            CollectionAssert.AreEquivalent(keyValuePairs, testCollection);

            foreach (var key in keyValuePairs.Take(5).Select(pair => pair.Key))
            {
                testCollection.Remove(key);
            }

            // Test collections are equivalent
            Assert.AreEqual(5, testCollection.Count);
            CollectionAssert.AreEquivalent(keyValuePairs.Skip(5).ToList(), testCollection);
        }

        [TestMethod]
        public void AddRemoveRangeTest()
        {
            var keyValuePairs = Enumerable.Range(0, 10)
                .Select(i => new KeyValuePair<object, int>(new object(), i)).ToArray();

            var testCollection = new ConcurrentObservableDictionary<object, int>();
            foreach (var pair in keyValuePairs)
            {
                testCollection.Add(pair.Key, pair.Value);
            }

            // Test collections are equivalent
            CollectionAssert.AreEquivalent(keyValuePairs, testCollection);

            testCollection.RemoveRange(keyValuePairs.Take(5).Select(pair => pair.Key));

            // Test collections are equivalent
            Assert.AreEqual(5, testCollection.Count);
            CollectionAssert.AreEquivalent(keyValuePairs.Skip(5).ToList(), testCollection);

        }

        [TestMethod]
        public void AddRemoveTest()
        {
            var keyValuePairs = Enumerable.Range(0, 10)
                .Select(i => new KeyValuePair<object, int>(new object(), i)).ToArray();

            var testCollection = new ConcurrentObservableDictionary<object, int>();
            foreach (var pair in keyValuePairs)
            {
                testCollection.Add(pair.Key, pair.Value);
            }

            // Test collections are equivalent
            CollectionAssert.AreEquivalent(keyValuePairs, testCollection);

            foreach (var key in keyValuePairs.Take(5).Select(pair => pair.Key))
            {
                testCollection.Remove(key);
            }

            // Test collections are equivalent
            Assert.AreEqual(5, testCollection.Count);
            CollectionAssert.AreEquivalent(keyValuePairs.Skip(5).ToList(), testCollection);
        }
    }
}
