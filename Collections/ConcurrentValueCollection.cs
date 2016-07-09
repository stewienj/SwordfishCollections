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
  /// This holds a collection of values that belong to a concurrent dictionary
  /// </summary>
  public class ConcurrentValueCollection<TKey, TValue> : ImmutableCollectionBase<TValue> {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// The immutable list of key value pairs from the dictionary
    /// </summary>
    private ICollection<KeyValuePair<TKey, TValue>> _pairs;

    #endregion Private Fields

    // ************************************************************************
    // Constructors
    // ************************************************************************
    #region Constructors

    /// <summary>
    /// Constructor that takes a dictionary
    /// </summary>
    public ConcurrentValueCollection(ConcurrentObservableDictionary<TKey, TValue> dictionary) {
      _pairs = dictionary.Snapshot;
    }

    #endregion Constructors

    // ************************************************************************
    // ImmutableCollectionBase implementation
    // ************************************************************************
    #region ImmutableCollectionBase implementation

    /// </summary>
    /// <param name="item">The object to locate</param>
    /// <returns>true if item is found otherwise false</returns>
    public override bool Contains(TValue item) {
      foreach(var pair in _pairs){
      if (item.Equals(pair.Value)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    //  Copies the elements of the collection to an array, starting
    /// at a particular index.
    /// </summary>
    public override void CopyTo(TValue[] array, int arrayIndex) {
      if (array == null) {
        throw (new System.ArgumentNullException());
      }
      foreach(var pair in _pairs){
        array[arrayIndex] = pair.Value;
        ++arrayIndex;
      }
    }

    /// <summary>
    /// Gets the enumerator for the collection
    /// </summary>
    public override IEnumerator<TValue> GetEnumerator() {
      foreach (var pair in _pairs) {
        yield return pair.Value;
      }
    }

    /// <summary>
    /// Gets the number of elements contained in the collection<T>.
    /// </summary>
    public override int Count {
      get {
        return _pairs.Count;
      }
    }

    #endregion ImmutableCollectionBase implementation
  }
}
