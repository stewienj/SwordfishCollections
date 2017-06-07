// Authored by: John Stewien
// Year: 2011
// Company: Swordfish Computing
// License: 
// The Code Project Open License http://www.codeproject.com/info/cpol10.aspx
// Originally published at:
// http://www.codeproject.com/Articles/208361/Concurrent-Observable-Collection-Dictionary-and-So
// Last Revised: September 2012

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.Collections {
  public class DoubleLinkListDictionaryNode<TKey,TValue> {
    public DoubleLinkListDictionaryNode<TKey, TValue> Next;
    public DoubleLinkListDictionaryNode<TKey, TValue> Previous;
    public TKey Key;
    public TValue Value;

    public DoubleLinkListDictionaryNode(TKey key, TValue value, DoubleLinkListDictionaryNode<TKey, TValue> next) {
      Next = next;
      Key = key;
      Value = value;
      if(Next != null) {
        Next.Previous = this;
      }
    }

    public override string ToString() {
      return string.Format("{0}, {1}", Key, Value);
    }


  }
}
