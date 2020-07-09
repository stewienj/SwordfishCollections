using System.Collections.Generic;

namespace Swordfish.NET.Collections
{
    /// <summary>
    /// Builder for KeyValuePair<TKey,TValue></TKey>
    /// </summary>
    public static class KeyValuePair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
