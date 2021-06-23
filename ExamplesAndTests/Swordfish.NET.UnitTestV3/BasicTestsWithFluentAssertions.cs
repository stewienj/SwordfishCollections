using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class BasicTestsWithFluentAssertions
    {

        //A demonstration of unit tests using xunit
        //Each test is done using just xUnit, then repeated using the FluentAssertions library
        //These are not necessarily good units tests just a demonstration of how to do them

        [TestMethod]
        public void ReadOnlyTest()
        { //using just xUnit
            var collection = new ConcurrentObservableCollection<int>();
            Assert.IsFalse(collection.IsReadOnly);
        }

        [TestMethod]
        public void ReadOnlyTestFluent()
        { //Using the fluent assertions library
            var collection = new ConcurrentObservableCollection<int>();
            collection.IsReadOnly.Should().BeFalse();
        }

        [TestMethod]
        public void CountTest()
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(new int[] { 1, 2, 3 });
            //xUnit
            Assert.IsTrue(collection.Count == 3, "should have count of three");
        }
        [TestMethod]
        public void CountTestFluent()
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(new int[] { 1, 2, 3 });
            //fluentAssertion
            collection.Should().HaveCount(3, "because we put three items in it");
        }

        [TestMethod]
        public void ContentTest()
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(new int[] { 1, 2, 3 });
            Assert.IsTrue(collection.Any(i => i == 1 || i == 2 || i == 3));
        }

        [TestMethod]
        public void ContentTestFluent()
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(new int[] { 1, 2, 3 });
            collection.Should().Contain(1).And.Contain(2).And.Contain(3);
        }


        //Using xUnit 'Theory' for testing, can put in multiple input data and test on each
        [DataTestMethod]
        [DataRow(new int[] { 1, 2, 3 })]
        [DataRow(new int[] { 0 })]
        public void ContentTheoryTest(int[] array)
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(array);
            Assert.IsTrue(collection.Any());
        }
        [DataTestMethod]
        [DataRow(new int[] { 1, 2, 3 })]
        [DataRow(new int[] { 0 })]
        public void ContentTheoryTestFluent(int[] array)
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(array);
            collection.Should().NotBeNullOrEmpty();
        }
    }
}
