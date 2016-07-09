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
using System.Collections;

namespace Swordfish.NET.Collections {
  /// <summary>
  /// This holds a collection of keys that belong to a concurrent dictionary
  /// </summary>
  public class ConcurrentKeyCollection<TKey, TValue> : ImmutableCollectionBase<TKey> {

    /// <summary>
    /// The immutable list of key value pairs from the dictionary
    /// </summary>
    private ICollection<KeyValuePair<TKey, TValue>> _pairs;

    /// <summary>
    /// Constructor that takes a dictionary
    /// </summary>
    public ConcurrentKeyCollection(ConcurrentObservableDictionary<TKey, TValue> dictionary) {
      _pairs = dictionary.Snapshot;
    }

    /// <summary>
    /// Gets the number of elements contained in the collection<T>.
    /// </summary>
    public override int Count {
      get {
        return _pairs.Count;
      }
    }

    /// </summary>
    /// <param name="item">The object to locate</param>
    /// <returns>true if item is found otherwise false</returns>
    public override bool Contains(TKey item) {
      foreach (var pair in _pairs) {
        if (item.Equals(pair.Key)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    //  Copies the elements of the collection to an array, starting
    /// at a particular index.
    /// </summary>
    public override void CopyTo(TKey[] array, int arrayIndex) {
      if (array == null) {
        throw (new System.ArgumentNullException());
      }

      foreach(var pair in _pairs){
        array[arrayIndex] = pair.Key;
        ++arrayIndex;
      }
    }

    /// <summary>
    /// Gets the enumerator for the collection
    /// </summary>
    public override IEnumerator<TKey> GetEnumerator() {
      foreach(var pair in _pairs){
        yield return pair.Key;
      }
    }
  }
}
