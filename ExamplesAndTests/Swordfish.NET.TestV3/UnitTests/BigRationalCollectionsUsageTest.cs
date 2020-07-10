using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections.Auxiliary;
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
        private readonly int _iterations = 5_000;
        private readonly int _count = 1_000;

        [TestMethod]
        public void TestIteratingOnInsertionIndexDownDirectionAlternate1()
        {
            // If iterations is set to 10_000 it takes 23 minutes to run this test

            List<BigRationalOld> testSetLower = new List<BigRationalOld>(_count);
            List<BigRationalOld> testSetUpper = new List<BigRationalOld>(_count);
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
                    BigRationalOld newValue = (testSetLower[i] + testSetUpper[i]) / 2;
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

            List<BigRationalOld> testSetLower = new List<BigRationalOld>(_count);
            List<BigRationalOld> testSetUpper = new List<BigRationalOld>(_count);
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
                    BigRationalOld newValue = (testSetLower[i] + testSetUpper[i]) / 2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetLower[i] = newValue;
                }
            }
        }


        [TestMethod]
        public void TestIteratingOnInsertionIndexDownDirectionAlternate2()
        {
            List<BigRational> testSetLower = new List<BigRational>(_count);
            List<BigRational> testSetUpper = new List<BigRational>(_count);
            for (int i = 0; i < _count; ++i)
            {
                testSetLower.Add((BigRational)i);
                testSetUpper.Add((BigRational)(i + 1));
            }
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                for (int i = 0; i < _count; ++i)
                {
                    // Get pairs of number, take half way between them, and check it
                    // is less than the high end and greater than the lower end
                    BigRational newValue = (testSetLower[i] + testSetUpper[i]) / (BigRational)2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetUpper[i] = newValue;
                }
            }
        }

        [TestMethod]
        public void TestIteratingOnInsertionIndexUpDirectionAlternate2()
        {
            List<BigRational> testSetLower = new List<BigRational>(_count);
            List<BigRational> testSetUpper = new List<BigRational>(_count);
            for (int i = 0; i < _count; ++i)
            {
                testSetLower.Add((BigRational)i);
                testSetUpper.Add((BigRational)(i + 1));
            }

            var half = (BigRational)0.5;
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                for (int i = 0; i < _count; ++i)
                {
                    // Get pairs of number, take half way between them, and check it
                    // is less than the high end and greater than the lower end
                    BigRational newValue = (testSetLower[i] + testSetUpper[i]) / (BigRational)2;
                    Assert.IsTrue(newValue > testSetLower[i]);
                    Assert.IsTrue(newValue < testSetUpper[i]);
                    testSetLower[i] = newValue;
                }
            }
        }
    }
}
