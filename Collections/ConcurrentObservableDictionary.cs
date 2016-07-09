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
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Swordfish.NET.Collections {

  /// <summary>
  /// This class provides a dictionary that can be bound to a WPF control,
  /// where the dictionary can be modified from a thread that is not the
  /// GUI thread. The notify event is thrown using the dispatcher from
  /// the event listener(s).
  /// </summary>
  /// <remarks>
  /// Used a Dictionary to map keys to the index in the underlying observable
  /// collection.
  /// 
  /// TODO: Implement all of the methods offered by the framework
  /// ConcurrentDictionary.
  /// </remarks>
  public class ConcurrentObservableDictionary<TKey, TValue> :
    ConcurrentObservableBase<KeyValuePair<TKey, TValue>>,
    IDictionary<TKey, TValue>,
    ICollection<KeyValuePair<TKey, TValue>>,
    ICollection {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// A dictionary of link list nodes to work out for the key the corresponding
    /// index for the master list, key list, and value list.
    /// </summary>
    protected Dictionary<TKey, DoubleLinkListIndexNode> _keyToIndex;
    /// <summary>
    /// The last node of the link list, used for adding new nodes to the end
    /// </summary>
    protected DoubleLinkListIndexNode _lastNode = null;

    #endregion Private Fields

    // ************************************************************************
    // Public Methods
    // ************************************************************************
    #region Public Methods

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the default
    /// initial capacity, and uses the default equality comparer for the key
    /// type.
    /// </summary>
    public ConcurrentObservableDictionary() {
      _keyToIndex = new Dictionary<TKey, DoubleLinkListIndexNode>();
    }

    /// <summary>
    /// Initializes a new instance of this class that contains elements copied
    /// from the specified IDictionary<TKey, TValue> and uses the default
    /// equality comparer for the key type.
    /// </summary>
    /// <param name="source"></param>
    public ConcurrentObservableDictionary(IDictionary<TKey, TValue> source)
      : this() {
      
      foreach (KeyValuePair<TKey, TValue> pair in source) {
        Add(pair);
      }
    }

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the default
    /// initial capacity, and uses the specified IEqualityComparer<T>.
    /// </summary>
    /// <param name="equalityComparer"></param>
    public ConcurrentObservableDictionary(IEqualityComparer<TKey> equalityComparer)
      : this() {
      
      _keyToIndex = new Dictionary<TKey, DoubleLinkListIndexNode>(equalityComparer);
    }

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the
    /// specified initial capacity, and uses the default equality comparer for
    /// the key type.
    /// </summary>
    /// <param name="capactity"></param>
    public ConcurrentObservableDictionary(int capactity)
      : this() {

      _keyToIndex = new Dictionary<TKey, DoubleLinkListIndexNode>(capactity);
    }

    /// <summary>
    /// Initializes a new instance of this class that contains elements copied
    /// from the specified IDictionary<TKey, TValue> and uses the specified
    /// IEqualityComparer<T>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="equalityComparer"></param>
    public ConcurrentObservableDictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> equalityComparer)
      : this(equalityComparer) {

      foreach (KeyValuePair<TKey, TValue> pair in source) {
        Add(pair);
      }
    }

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the
    /// specified initial capacity, and uses the specified
    /// IEqualityComparer<T>.
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="equalityComparer"></param>
    public ConcurrentObservableDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
      : this() {

        _keyToIndex = new Dictionary<TKey, DoubleLinkListIndexNode>(capacity, equalityComparer);
    }

    /// <summary>
    /// Gets the array index of the key passed in.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int IndexOfKey(TKey key) {
      return DoBaseRead(() => {
        return _keyToIndex[key].Index;
      });
    }

    /// <summary>
    /// Tries to get the index of the key passed in. Returns true if succeeded
    /// and false otherwise.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool TryGetIndexOf(TKey key, out int index) {
      var values = DoBaseRead(() => {
        DoubleLinkListIndexNode node;
        if (_keyToIndex.TryGetValue(key, out node)) {
          return Tuple.Create(true, node.Index);
        } else {
          return Tuple.Create(false, 0);
        }
      });
      index = values.Item2;
      return values.Item1;
    }

    #endregion Public Methods

    // ************************************************************************
    // IDictionary<TKey, TValue> Members
    // ************************************************************************
    #region IDictionary<TKey, TValue> Members

    /// <summary>
    /// Adds an element with the provided key and value to the IDictionary<TKey, TValue>.
    /// </summary>
    /// <param name="key">
    /// The object to use as the key of the element to add.
    /// </param>
    /// <param name="value">
    /// The object to use as the value of the element to add.
    /// </param>
    public void Add(TKey key, TValue value) {
      DoBaseWrite(() => {
        BaseAdd(key, value);
      });
    }

    /// <summary>
    /// Tries adding a new key value pair. 
    /// </summary>
    /// <param name="key">
    /// The object to use as the key of the element to add.
    /// </param>
    /// <param name="value">
    /// The object to use as the value of the element to add.
    /// </param>
    /// <returns>
    /// True if successful or false if the key already exists.
    /// </returns>
    public bool TryAdd(TKey key, TValue value) {
      return DoBaseReadWrite(() => {
        return _keyToIndex.ContainsKey(key);
      }, () => {
        return false;
      }, () => {
        BaseAdd(key, value);
        return true;
      });
    }

    /// <summary>
    /// Retrives a value for the key passed in if it exists, else
    /// adds the new value passed in.
    /// </summary>
    /// <param name="key">
    /// The object to use as the key of the element to retrieve or add.
    /// </param>
    /// <param name="value">
    /// The object to add if it doesn't already exist
    /// </param>
    /// <returns></returns>
    public TValue RetrieveOrAdd(TKey key, Func<TValue> getValue) {
      TValue value = default(TValue);
      return DoBaseReadWrite(() => {
        // Test for read or write
        return _keyToIndex.ContainsKey(key);
      }, () => {
        // Read func
        int index = _keyToIndex[key].Index;
        return WriteCollection[index].Value;
      }, () => {
        // Pre write func (outside of locking the collection)
        value = getValue();
      }, () => {
        // Write func
        BaseAdd(key, value);
        return value;
      });
    }

    protected virtual void BaseAdd(TKey key, TValue value) {
      DoubleLinkListIndexNode node = new DoubleLinkListIndexNode(_lastNode, _keyToIndex.Count);
      _keyToIndex.Add(key, node);
      _lastNode = node;
      WriteCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Determines whether the IDictionary<TKey, TValue> contains an element with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key to locate in the IDictionary<TKey, TValue>.
    /// </param>
    /// <returns>
    /// True if the IDictionary<TKey, TValue> contains an element with the key; otherwise, false.
    /// </returns>
    public bool ContainsKey(TKey key) {
      return DoBaseRead(() => {
        return _keyToIndex.ContainsKey(key);
      });
    }

    /// <summary>
    /// Gets an ICollection<T> containing the keys of the IDictionary<TKey, TValue>.
    /// </summary>
    /// <remarks>
    /// Note that the returned collection is immutable. This was deliberate, and
    /// is related to the implementation of GetEnumerator() which also returns a
    /// snapshot.
    /// 
    /// If you hand off the Enumerator, Values, or Keys to a GUI object you'll
    /// potentially get a crash because the collection can be modified by another
    /// thread while the GUI is enumerating.
    /// 
    /// I thought about having an enumerator where the underlying collection
    /// could be modified without causing an exception, and have new objects
    /// added to the end, but then how would that work for the sorted version
    /// (which was actually my end goal)?
     /// </remarks>
    public ICollection<TKey> Keys {
      get {
        return new ConcurrentKeyCollection<TKey, TValue>(this);
      }
    }

    /// <summary>
    /// Removes the element with the specified key from the IDictionary<TKey, TValue>.
    /// </summary>
    /// <param name="key">
    /// The key of the element to remove.
    /// </param>
    /// <returns>
    /// True if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the original IDictionary<TKey, TValue>.
    /// </returns>
    public bool Remove(TKey key) {
      return DoBaseWrite(() => {
        DoubleLinkListIndexNode node;
        if (_keyToIndex.TryGetValue(key, out node)) {
          WriteCollection.RemoveAt(node.Index);
          if (node == _lastNode) {
            _lastNode = node.Previous;
          }
          node.Remove();
        }
        return _keyToIndex.Remove(key);
      });
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key whose value to get.
    /// </param>
    /// <param name="value">
    /// When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// True if the object that implements IDictionary<TKey, TValue> contains an element with the specified key; otherwise, false.
    /// </returns>
    public bool TryGetValue(TKey key, out TValue value) {
      var values = DoBaseRead(() => {
        DoubleLinkListIndexNode index;
        if (_keyToIndex.TryGetValue(key, out index)) {
          // Use write collection here because that is what
          // is in sync with _keyToIndex
          return Tuple.Create(true, WriteCollection[index.Index].Value);
        } else {
          return Tuple.Create(false, default(TValue));
        }
      });
      value = values.Item2;
      return values.Item1;
    }

    /// <summary>
    /// Gets an ICollection<T> containing the values in the IDictionary<TKey, TValue>.
    /// </summary>
    /// <remarks>
    /// Note that the returned collection is immutable. This was deliberate, and
    /// is related to the implementation of GetEnumerator() which also returns a
    /// snapshot.
    /// 
    /// If you hand off the Enumerator, Values, or Keys to a GUI object you'll
    /// potentially get a crash because the collection can be modified by another
    /// thread while the GUI is enumerating.
    /// 
    /// I thought about having an enumerator where the underlying collection
    /// could be modified without causing an exception, and have new objects
    /// added to the end, but then how would that work for the sorted version
    /// (which was actually my end goal)?
    /// </remarks>
    public ICollection<TValue> Values {
      get {
        return new ConcurrentValueCollection<TKey, TValue>(this);
      }
    }

    /// <summary>
    /// Gets or sets the element with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key of the element to get or set.
    /// </param>
    /// <returns>
    /// The element with the specified key.
    /// </returns>
    public TValue this[TKey key] {
      get {
        return DoBaseRead(() => {
          int index = _keyToIndex[key].Index;
          // Use write collection here because that is what
          // is in sync with _keyToIndex
          return WriteCollection[index].Value;
        });
      }
      set {
        DoBaseWrite(() => {
          if (_keyToIndex.ContainsKey(key)) {
            int index = _keyToIndex[key].Index;
            WriteCollection[index] = new KeyValuePair<TKey, TValue>(key, value);
          } else {
            BaseAdd(key, value);
          }
        });
      }
    }

    #endregion IDictionary<TKey, TValue> Members

    // ************************************************************************
    // ICollection<KeyValuePair<TKey, TValue>> Members
    // ************************************************************************
    #region ICollection<KeyValuePair<TKey, TValue>> Members

    /// <summary>
    /// Adds an item to the ICollection<T>.
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<TKey, TValue> item) {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Removes all items from the ICollection<T>.
    /// </summary>
    public void Clear() {
      DoBaseClear(()=>{
        // See base class implementation
        _keyToIndex.Clear();
        _lastNode = null;
      });
    }

    /// <summary>
    /// Determines whether the ICollection<T> contains a specific value.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<TKey, TValue> item) {
      return DoBaseRead(() => {
        return ReadCollection.Contains(item);
      });
    }

    /// <summary>
    /// Copies the elements of the ICollection<T> to an Array, starting at a particular Array index.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      DoBaseRead(() => {
        ReadCollection.CopyTo(array, arrayIndex);
      });
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the ICollection<T>.
    /// </summary>
    /// <param name="item">
    /// The object to remove from the ICollection<T>.
    /// </param>
    /// <returns>
    /// True if item was successfully removed from the ICollection<T>; otherwise, false. This method also returns false if item is not found in the original ICollection<T>.
    /// </returns>
    public bool Remove(KeyValuePair<TKey, TValue> item) {
      // "Contains" does a read lock, and "Remove" does a write lock..
      // ... so don't want to wrap this in a lock
      if (Contains(item)) {
        return Remove(item.Key);
      } else {
        return false;
      }
    }

    /// <summary>
    /// Gets the number of elements contained in the ICollection<T>.
    /// </summary>
    public int Count {
      get {
        return DoBaseRead(() => {
          return ReadCollection.Count;
        });
      }
    }

    /// <summary>
    /// Gets a value indicating whether the ICollection<T> is read-only.
    /// </summary>
    public bool IsReadOnly {
      get {
        return false;
      }
    }

    #endregion ICollection<KeyValuePair<TKey, TValue>> Members

    // ************************************************************************
    // ICollection Members
    // ************************************************************************
    #region ICollection Members

    /// <summary>
    /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    public void CopyTo(Array array, int index) {
      DoBaseRead(() => {
        ((ICollection)ReadCollection).CopyTo(array, index);
      });
    }
 
    /// <summary>
    /// Gets a value indicating whether access to the ICollection is synchronized (thread safe).
    /// </summary>
    public bool IsSynchronized {
      get {
        return DoBaseRead(() => {
          return ((ICollection)ReadCollection).IsSynchronized;
        });
      }
    }

    /// <summary>
    /// Gets an object that can be used to synchronize access to the ICollection.
    /// </summary>
    public object SyncRoot {
      get {
        return DoBaseRead(() => {
          return ((ICollection)ReadCollection).SyncRoot;
        });
      }
    }

    #endregion ICollection Members

  }
}
