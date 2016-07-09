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

namespace Swordfish.NET.Collections {
  public class MostRecentlyUsedDictionary<TKey, TValue> {

    // ************************************************************************
    // Nested Classes
    // ************************************************************************
    #region Nested Classes

    protected class DictionaryNode : DoubleLinkListDictionaryNode<TKey, TValue> {

      public DictionaryNode(TKey key, TValue value, DoubleLinkListDictionaryNode<TKey, TValue> next)
        : base(key, value, next) {
      }
    }

    #endregion Nested Classes

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// A dictionary of link list nodes to work out for the key the corresponding
    /// index for the master list, key list, and value list.
    /// </summary>
    protected Dictionary<TKey, DictionaryNode> _keyToIndex;
    /// <summary>
    /// The last node of the link list, used for removing old nodes from the end
    /// </summary>
    protected DoubleLinkListDictionaryNode<TKey, TValue> _lastNode = null;
    /// <summary>
    /// The first node, for adding new nodes to the beginning
    /// </summary>
    protected DoubleLinkListDictionaryNode<TKey, TValue> _firstNode = null;

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
    public MostRecentlyUsedDictionary() {
      _keyToIndex = new Dictionary<TKey, DictionaryNode>();
    }

    /// <summary>
    /// Initializes a new instance of this class that contains elements copied
    /// from the specified IDictionary<TKey, TValue> and uses the default
    /// equality comparer for the key type.
    /// </summary>
    /// <param name="source"></param>
    public MostRecentlyUsedDictionary(IDictionary<TKey, TValue> source)
      : this() {
      
      foreach (KeyValuePair<TKey, TValue> pair in source) {
        Add(pair.Key, pair.Value);
      }
    }

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the default
    /// initial capacity, and uses the specified IEqualityComparer<T>.
    /// </summary>
    /// <param name="equalityComparer"></param>
    public MostRecentlyUsedDictionary(IEqualityComparer<TKey> equalityComparer)
      : this() {

        _keyToIndex = new Dictionary<TKey, DictionaryNode>(equalityComparer);
    }

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the
    /// specified initial capacity, and uses the default equality comparer for
    /// the key type.
    /// </summary>
    /// <param name="capactity"></param>
    public MostRecentlyUsedDictionary(int capactity)
      : this() {

        _keyToIndex = new Dictionary<TKey, DictionaryNode>(capactity);
    }

    /// <summary>
    /// Initializes a new instance of this class that contains elements copied
    /// from the specified IDictionary<TKey, TValue> and uses the specified
    /// IEqualityComparer<T>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="equalityComparer"></param>
    public MostRecentlyUsedDictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> equalityComparer)
      : this(equalityComparer) {

      foreach (KeyValuePair<TKey, TValue> pair in source) {
        Add(pair.Key, pair.Value);
      }
    }

    /// <summary>
    /// Initializes a new instance of this class that is empty, has the
    /// specified initial capacity, and uses the specified
    /// IEqualityComparer<T>.
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="equalityComparer"></param>
    public MostRecentlyUsedDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
      : this() {

        _keyToIndex = new Dictionary<TKey, DictionaryNode>(capacity, equalityComparer);
    }

    #endregion Public Methods

    // ************************************************************************
    // Subset of IDictionary<TKey, TValue> Members
    // ************************************************************************
    #region Subset of IDictionary<TKey, TValue> Members

    /// <summary>
    /// Adds an element with the provided key and value to the IDictionary<TKey, TValue>.
    /// </summary>
    /// <param name="key">
    /// The object to use as the key of the element to add.
    /// </param>
    /// <param name="value">
    /// The object to use as the value of the element to add.
    /// </param>
    public virtual void Add(TKey key, TValue value) {
      DictionaryNode node = new DictionaryNode(key, value, _firstNode);
      _keyToIndex.Add(key, node);
      _firstNode = node;
      if(_lastNode == null)
        _lastNode = node;
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
      return _keyToIndex.ContainsKey(key);
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
      DictionaryNode node;
      if (_keyToIndex.TryGetValue(key, out node)){
        RemoveNode(node);
      }
      return _keyToIndex.Remove(key);
    }

    private void RemoveNode(DoubleLinkListDictionaryNode<TKey, TValue> node) {
      if(node == _lastNode) {
        _lastNode = node.Previous;
      }
      if(node == _firstNode) {
        _firstNode = node.Next;
      }

      if(node.Next != null) {
        node.Next.Previous = node.Previous;
      }
      if(node.Previous != null) {
        node.Previous.Next = node.Next;
      }
      node.Next = null;
      node.Previous = null;
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
      DictionaryNode node;
      if (_keyToIndex.TryGetValue(key, out node)) {
        value = node.Value;
        Touch(node);
        return true;
      } else {
        value = default(TValue);
        return false;
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
        DictionaryNode node = _keyToIndex[key];
        Touch(node);
        return node.Value;
      }
      set {
        if(ContainsKey(key)) {
          DictionaryNode node = _keyToIndex[key];
          Touch(node);
          node.Value = value;
        } else {
          Add(key, value);
        }
      }
    }

    #endregion Subset of IDictionary<TKey, TValue> Members

    private void Touch(DictionaryNode node) {
      RemoveNode(node);
      node.Next = _firstNode;
      if (_firstNode != null)
        _firstNode.Previous = node;
      _firstNode = node;
    }

    public TValue LeastRecentlyUsed {
      get {
        if(_lastNode != null) {
          return _lastNode.Value;
        } else {
          return default(TValue);
        }
      }
    }

    public void TrimLeastRecentlyUsed() {
      if(_lastNode != null) {
        _keyToIndex.Remove(_lastNode.Key);
        RemoveNode(_lastNode);
      }
    }
    /// <summary>
    /// Removes all items from the ICollection<T>.
    /// </summary>
    public void Clear() {
      _keyToIndex.Clear();
      _lastNode = null;
    }

    /// <summary>
    /// Gets the number of elements contained in the ICollection<T>.
    /// </summary>
    public int Count {
      get {
        return _keyToIndex.Count;
      }
    }
  }
}
