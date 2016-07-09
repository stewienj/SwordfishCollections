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
  /// This class provides a sorted collection that can be bound to a WPF
  /// control, where the dictionary can be modified from a thread that is
  /// not the GUI thread. The notify event is thrown using the dispatcher
  /// from the event listener(s).
  /// </summary>
  public class ConcurrentObservableSortedDictionary<TKey, TValue> : ConcurrentObservableDictionary<TKey, TValue> {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// Used for doing a sorted insert of new entries
    /// </summary>
    private BinarySorter<TKey> _sorter;

    #endregion Private Fields

    // ************************************************************************
    // Public Methods
    // ************************************************************************
    #region Public Methods

    /// <summary>
    /// Constructor with an optional IComparer<TKey> parameter.
    /// </summary>
    /// <param name="comparer">Comparer used to sort the keys.</param>
    public ConcurrentObservableSortedDictionary(IComparer<TKey> comparer = null) {
      _sorter = new BinarySorter<TKey>(comparer);
    }

    /// <summary>
    /// Adds an element with the provided key and value to the IDictionary<TKey, TValue>.
    /// </summary>
    /// <param name="key">
    /// The object to use as the key of the element to add.
    /// </param>
    /// <param name="value">
    /// The object to use as the value of the element to add.
    /// </param>
    protected override void BaseAdd(TKey key, TValue value) {
      int index = _sorter.GetInsertIndex(WriteCollection.Count, key, delegate(int testIndex) {
        return WriteCollection[testIndex].Key;
      });

      if(index >= WriteCollection.Count) {
        base.BaseAdd(key, value);
      } else {
        TKey listKey = WriteCollection[index].Key;
        DoubleLinkListIndexNode next = _keyToIndex[listKey];
        DoubleLinkListIndexNode newNode = new DoubleLinkListIndexNode(next.Previous, next);
        _keyToIndex[key] = newNode;
        WriteCollection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
      }
    }

    #endregion
  }
}
