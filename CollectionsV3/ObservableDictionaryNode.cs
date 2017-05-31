using Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections
{
  public class ObservableDictionaryNode<TKey, TValue>
  {
    public ObservableDictionaryNode(KeyValuePair<TKey, TValue> pair, BigRational position)
    {
      KeyValuePair = pair;
      SortKey = position;
    }

    public ObservableDictionaryNode(KeyValuePair<TKey, TValue> pair, ObservableDictionaryNode<TKey, TValue> before)
      : this(pair, before!=null ? before.SortKey+1 : 0) { }
 

    public KeyValuePair<TKey, TValue> KeyValuePair { get; }

    public TKey Key { get { return KeyValuePair.Key; } }
    public TValue Value { get { return KeyValuePair.Value; } }
    /// <summary>
    /// Position is a BigRational that can be used to do a binary search for a node
    /// </summary>
    public BigRational SortKey { get; }

  }
}
