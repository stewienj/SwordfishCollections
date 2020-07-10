using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Swordfish.NET.TestV3.UnitTests
{
    /// <summary>
    /// The Swordfish.NET.Collections.Auxiliary.BigRational class is used in the collections as a sort key.
    /// When an item is to be inserted it is given a key that is half
    /// way between the keys of other other 2 objects it is inserted between.
    /// </summary>
    [TestClass]
    public class BigRationalCollectionsUsageTest
    {
        private readonly int _iterations = 4_000;
        private readonly int _count = 1_000;

        [TestMethod]
        public void TestIteratingOnInsertionIndexDownDirectionAlternate1()
        {
            // If iterations is set to 10_000 it takes 23 minutes to run this test

            List<Swordfish.NET.Collections.Auxiliary.BigRational> testSetLower = new List<Swordfish.NET.Collections.Auxiliary.BigRational>(_count);
            List<Swordfish.NET.Collections.Auxiliary.BigRational> testSetUpper = new List<Swordfish.NET.Collections.Auxiliary.BigRational>(_count);
            for (int i = 0; i < _count; ++i)
            {
                testSetLower.Add(i);
                testSetUpper.Add(i + 1);
            }
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                for (int i = 0; i < _count; ++i)
                {
                    // Get pairs of number, take half way between them, and check it
                    // is less than the high end and greater than the lower end
                    Swordfish.NET.Collections.Auxiliary.BigRational newValue = (testSetLower[i] + testSetUpper[i]) / 2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetUpper[i] = newValue;
                }
            }
        }

        [TestMethod]
        public void TestIteratingOnInsertionIndexUpDirectionAlternate1()
        {
            // If iterations is set to 10_000 it takes 23 minutes to run this test

            List<Swordfish.NET.Collections.Auxiliary.BigRational> testSetLower = new List<Swordfish.NET.Collections.Auxiliary.BigRational>(_count);
            List<Swordfish.NET.Collections.Auxiliary.BigRational> testSetUpper = new List<Swordfish.NET.Collections.Auxiliary.BigRational>(_count);
            for (int i = 0; i < _count; ++i)
            {
                testSetLower.Add(i);
                testSetUpper.Add(i + 1);
            }
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                for (int i = 0; i < _count; ++i)
                {
                    // Get pairs of number, take half way between them, and check it
                    // is less than the high end and greater than the lower end
                    Swordfish.NET.Collections.Auxiliary.BigRational newValue = (testSetLower[i] + testSetUpper[i]) / 2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetLower[i] = newValue;
                }
            }
        }


        [TestMethod]
        public void TestIteratingOnInsertionIndexDownDirectionAlternate2()
        {
            List<ExtendedNumerics.BigRational> testSetLower = new List<ExtendedNumerics.BigRational>(_count);
            List<ExtendedNumerics.BigRational> testSetUpper = new List<ExtendedNumerics.BigRational>(_count);
            for (int i = 0; i < _count; ++i)
            {
                testSetLower.Add((ExtendedNumerics.BigRational)i);
                testSetUpper.Add((ExtendedNumerics.BigRational)(i + 1));
            }
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                for (int i = 0; i < _count; ++i)
                {
                    // Get pairs of number, take half way between them, and check it
                    // is less than the high end and greater than the lower end
                    ExtendedNumerics.BigRational newValue = (testSetLower[i] + testSetUpper[i]) / (ExtendedNumerics.BigRational)2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetUpper[i] = newValue;
                }
            }
        }

        [TestMethod]
        public void TestIteratingOnInsertionIndexUpDirectionAlternate2()
        {
            List<ExtendedNumerics.BigRational> testSetLower = new List<ExtendedNumerics.BigRational>(_count);
            List<ExtendedNumerics.BigRational> testSetUpper = new List<ExtendedNumerics.BigRational>(_count);
            for (int i = 0; i < _count; ++i)
            {
                testSetLower.Add((ExtendedNumerics.BigRational)i);
                testSetUpper.Add((ExtendedNumerics.BigRational)(i + 1));
            }

            var half = (ExtendedNumerics.BigRational)0.5;
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                for (int i = 0; i < _count; ++i)
                {
                    // Get pairs of number, take half way between them, and check it
                    // is less than the high end and greater than the lower end
                    ExtendedNumerics.BigRational newValue = (testSetLower[i] + testSetUpper[i]) / (ExtendedNumerics.BigRational)2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetLower[i] = newValue;
                }
            }
        }
    }
}
