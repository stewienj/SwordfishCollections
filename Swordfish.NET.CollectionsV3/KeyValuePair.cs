using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
