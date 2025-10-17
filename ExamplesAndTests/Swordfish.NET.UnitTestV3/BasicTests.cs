using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;
using System.Linq;

namespace Swordfish.NET.UnitTestV3
{
    [TestClass]
    public class BasicTests
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
        public void CountTest()
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(new int[] { 1, 2, 3 });
            //xUnit
            Assert.IsTrue(collection.Count == 3, "should have count of three");
        }

        [TestMethod]
        public void ContentTest()
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(new int[] { 1, 2, 3 });
            Assert.IsTrue(collection.Any(i => i == 1 || i == 2 || i == 3));
        }


        //Using xUnit 'Theory' for testing, can put in multiple input data and test on each
        [TestMethod]
        [DataRow(new int[] { 1, 2, 3 })]
        [DataRow(new int[] { 0 })]
        public void ContentTheoryTest(int[] array)
        {
            var collection = new ConcurrentObservableCollection<int>();
            collection.AddRange(array);
            Assert.IsTrue(collection.Any());
        }
    }
}
