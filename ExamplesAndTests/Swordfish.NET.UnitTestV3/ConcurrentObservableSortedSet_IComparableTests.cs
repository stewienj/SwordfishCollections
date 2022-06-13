using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class ConcurrentObservableSortedSet_IComparableTests
    {
        /// <summary>
        ///  Creates a random set of TestClass objects
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private IEnumerable<TestClass> GetRandomSet(int count)
        {
            var random = new Random(1);
            for (int i = 0; i < count; ++i)
            {
                yield return new TestClass
                {
                    SomeInt = random.Next(),
                    SomeString = random.Next().ToString()
                };
            }
        }

        [TestMethod]
        public void TestThrowsWithoutIComparable()
        {
            var collection = new ConcurrentObservableSortedSet<TestClass>();
            collection.Add(new TestClass { SomeInt = 1, SomeString = "Two" });
            Assert.ThrowsException<ArgumentException>
            (
                () => collection.Add(new TestClass { SomeInt = 2, SomeString = "Three" })
            );
        }


        [TestMethod]
        public void TestCustomIComparableByInt()
        {
            var comparer = TestClassComparer.CreateAsIntComparer();

            var collection = new ConcurrentObservableSortedSet<TestClass>(comparer);
            int count = 100;
            var list = GetRandomSet(count).ToList();
            foreach (var item in list)
            {
                collection.Add(item);
            }

            // Check that it is ordered by Int
            var sortedList = list.OrderBy(x => x.SomeInt).ToList();

            for (int i = 0; i < count; ++i)
            {
                Assert.AreEqual(sortedList[i], collection[i]);
            }

        }

        [TestMethod]
        public void TestCustomIComparableByString()
        {
            var comparer = TestClassComparer.CreateAsStringComparer();

            var collection = new ConcurrentObservableSortedSet<TestClass>(comparer);
            int count = 100;
            var list = GetRandomSet(count).ToList();
            foreach (var item in list)
            {
                collection.Add(item);
            }

            // Check that it is ordered by Int
            var sortedList = list.OrderBy(x => x.SomeString).ToList();

            for (int i = 0; i < count; ++i)
            {
                Assert.AreEqual(sortedList[i], collection[i]);
            }

        }

        public class TestClass
        {
            public int SomeInt { get; set; }
            public string SomeString { get; set; }
        }

        public class TestClassComparer : IComparer<TestClass>
        {
            private enum CompareMethodType
            {
                Unknown,
                CompareInt,
                CompareString,
            }

            private CompareMethodType _compareMethod = CompareMethodType.Unknown;

            public static TestClassComparer CreateAsIntComparer()
            {
                return new TestClassComparer(CompareMethodType.CompareInt);
            }

            public static TestClassComparer CreateAsStringComparer()
            {
                return new TestClassComparer(CompareMethodType.CompareString);
            }

            private TestClassComparer(CompareMethodType compareMethod)
            {
                _compareMethod = compareMethod;
            }

            public int Compare(TestClass x, TestClass y)
            {
                switch (_compareMethod)
                {
                    case CompareMethodType.CompareInt:
                        return x.SomeInt.CompareTo(y.SomeInt);
                    case CompareMethodType.CompareString:
                        return x.SomeString.CompareTo(y.SomeString);
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
