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
using System.Collections.ObjectModel;

namespace Swordfish.NET.Collections {
  /// <summary>
  /// This class provides a functional immutable collection
  /// </summary>
  public class ImmutableCollection<T> : ImmutableCollectionBase<T> {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// The base collection that is wrapped by this class to restrict access
    /// </summary>
    private IList<T> _baseCollection;

    #endregion Private Fields

    // ************************************************************************
    // Constructors
    // ************************************************************************
    #region Constructors

    public ImmutableCollection(IEnumerable<T> source) {
      _baseCollection = new List<T>(source);
    }

    public ImmutableCollection() {
      _baseCollection = new List<T>();
    }

    #endregion Constructors

    // ************************************************************************
    // ImmutableCollectionBase Implementation
    // ************************************************************************
    #region ImmutableCollectionBase Implementation

    public override int Count {
      get {
        return _baseCollection.Count;
      }
    }

    public override bool Contains(T item) {
      return _baseCollection.Contains(item);
    }

    public override void CopyTo(T[] array, int arrayIndex) {
      _baseCollection.CopyTo(array, arrayIndex);
    }

    public override IEnumerator<T> GetEnumerator() {
      return _baseCollection.GetEnumerator();
    }

    #endregion ImmutableCollectionBase Implementation
  }
}
