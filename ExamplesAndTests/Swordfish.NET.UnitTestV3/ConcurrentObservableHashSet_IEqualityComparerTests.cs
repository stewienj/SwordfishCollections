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
    public class ConcurrentObservableHashSet_IEqualityComparerTests
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
        public void TestEqualityComparer()
        {
            int count = 100;

            var collection1 = new ConcurrentObservableHashSet<TestClass>();
            var collection2 = new ConcurrentObservableHashSet<TestClass>(new TestClassAlwaysEqualComparer());
            var randomSet = GetRandomSet(count).ToList();
            foreach (var item in randomSet)
            {
                collection1.Add(item);
                collection2.Add(item);
            }

            // Collection 1 will have all the items
            Assert.AreEqual(count, collection1.Count);

            // Collection 2 will have just 1 item
            Assert.AreEqual(1, collection2.Count);
        }

        public class TestClass
        {
            public int SomeInt { get; set; }
            public string SomeString { get; set; }
        }

        public class TestClassAlwaysEqualComparer : IEqualityComparer<TestClass>
        {
            public bool Equals(TestClass x, TestClass y)
            {
                return true;
            }

            public int GetHashCode(TestClass obj)
            {
                return 1;
            }
        }
    }
}
