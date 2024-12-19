using System;
using System.Collections.Generic;
using System.Text;

namespace Swordfish.NET.UnitTestV3
{
    /// <summary>
    /// A test class for creating objects to be used as Keys in Dictionary tests.
    /// </summary>
    public class KeyStringTest
    {
        public KeyStringTest(string keyValue)
        {
            KeyValue = keyValue;
        }
        public string KeyValue { get; }
    }

    /// <summary>
    /// A test class for creating objects to be used as Keys in Dictionary tests.
    /// </summary>
    public class KeyStringWithOverride
    {
        public KeyStringWithOverride(string keyValue)
        {
            KeyValue = keyValue;
        }

        public override int GetHashCode()
        {
            return KeyValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is KeyStringWithOverride ex)
            {
                return KeyValue.Equals(ex.KeyValue);
            }
            else
            {
                return false;
            }

        }

        public string KeyValue { get; }
    }

    /// <summary>
    /// A test class for creating objects to be used as Keys in Dictionary tests.
    /// </summary>
    public class KeyIntTest
    {
        public KeyIntTest(int keyValue)
        {
            KeyValue = keyValue;
        }
        public int KeyValue { get; }
    }
}
