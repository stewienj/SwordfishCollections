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
using System.Collections.Specialized;
using System.Threading;
using System.ComponentModel;
using System.Collections;

namespace Swordfish.NET.Collections {

  /// <summary>
  /// This class provides a collection that can be bound to
  /// a WPF control, where the collection can be modified from a thread
  /// that is not the GUI thread. The notify event is thrown using the
  /// dispatcher from the event listener(s).
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ConcurrentObservableCollection<T> : 
    ConcurrentObservableBase<T>,
    IList<T>, 
    ICollection<T>, 
    IList, 
    ICollection {

    // ************************************************************************
    // Constructors
    // ************************************************************************
    #region Constructors

    /// <summary>
    /// Default Constructor
    /// </summary>
    public ConcurrentObservableCollection() {
    }

    /// <summary>
    /// Constructor that takes an enumerable from which the collection is populated
    /// </summary>
    /// <param name="enumerable"></param>
    public ConcurrentObservableCollection(IEnumerable<T> enumerable) : base(enumerable){
    }

    #endregion Constructors

    // ************************************************************************
    // IList<T> Implementation
    // ************************************************************************
    #region IList<T> Implementation

    public int IndexOf(T item) {
      return DoBaseRead(() => {
        return ReadCollection.IndexOf(item);
      });
    }

    public void Insert(int index, T item) {
      DoBaseWrite(() => {
        WriteCollection.Insert(index, item);
      });
    }

    public void RemoveAt(int index) {
      DoBaseWrite(() => {
        WriteCollection.RemoveAt(index);
      });
    }

    public T this[int index] {
      get {
        return DoBaseRead(() => {
          return ReadCollection[index];
        });
      }
      set {
        DoBaseWrite(() => {
          WriteCollection[index] = value;
        });
      }
    }

    #endregion IList<T> Implementation

    // ************************************************************************
    // ICollection<T> Implementation
    // ************************************************************************
    #region ICollection<T> Implementation

    public void Add(T item) {
      DoBaseWrite(() => {
        WriteCollection.Add(item);
      });
    }

    public void Clear(){
      DoBaseClear(() => { });
    }

    public bool Contains(T item) {
      return DoBaseRead(() => {
        return ReadCollection.Contains(item);
      });
    }

    public void CopyTo(T[] array, int arrayIndex) {
      DoBaseRead(() => {
        ReadCollection.CopyTo(array, arrayIndex);
      });
    }

    public int Count {
      get {
        return DoBaseRead(() => {
          return ReadCollection.Count;
        });
      }
    }

    public bool IsReadOnly {
      get {
        return DoBaseRead(() => {
          return ((ICollection<T>)ReadCollection).IsReadOnly;
        });
      }
    }

    public bool Remove(T item) {
      return DoBaseWrite(() => {
        return WriteCollection.Remove(item);
      });
    }

    #endregion ICollection<T> Implementation

    // ************************************************************************
    // ICollection Implementation
    // ************************************************************************
    #region ICollection Implementation

    void ICollection.CopyTo(Array array, int index) {
      DoBaseRead(() => {
        ((ICollection)ReadCollection).CopyTo(array, index);
      });
    }

    bool ICollection.IsSynchronized {
      get {
        return DoBaseRead(() => {
          return ((ICollection)ReadCollection).IsSynchronized;
        });
      }
    }

    object ICollection.SyncRoot {
      get {
        return DoBaseRead(() => {
          return ((ICollection)ReadCollection).SyncRoot;
        });
      }
    }

    #endregion ICollection Implementation

    // ************************************************************************
    // IList Implementation
    // ************************************************************************
    #region IList Implementation

    int IList.Add(object value) {
      return DoBaseWrite(() => {
        return ((IList)WriteCollection).Add(value);
      });
    }

    bool IList.Contains(object value) {
      return DoBaseRead(() => {
        return ((IList)ReadCollection).Contains(value);
      });
    }

    int IList.IndexOf(object value) {
      return DoBaseRead(() => {
        return ((IList)ReadCollection).IndexOf(value);
      });
    }

    void IList.Insert(int index, object value) {
      DoBaseWrite(() => {
        ((IList)WriteCollection).Insert(index, value);
      });
    }

    bool IList.IsFixedSize {
      get {
        return DoBaseRead(() => {
          return ((IList)ReadCollection).IsFixedSize;
        });
      }
    }

    bool IList.IsReadOnly {
      get {
        return DoBaseRead(() => {
          return ((IList)ReadCollection).IsReadOnly;
        });
      }
    }

    void IList.Remove(object value) {
      DoBaseWrite(() => {
        ((IList)WriteCollection).Remove(value);
      });
    }

    void IList.RemoveAt(int index) {
      DoBaseWrite(() => {
        ((IList)WriteCollection).RemoveAt(index);
      });
    }

    object IList.this[int index] {
      get {
        return DoBaseRead(() => {
          return ((IList)ReadCollection)[index];
        });
      }
      set {
        DoBaseWrite(() => {
          ((IList)WriteCollection)[index] = value;
        });
      }
    }

    #endregion IList Implementation

  }
}
