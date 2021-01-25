using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace Swordfish.NET.Collections
{
    /// <summary>
    /// A collection that can be updated from multiple threads, and can be bound to an items control in the user interface.
    /// Has the advantage over ObservableCollection in that it doesn't have to be updated from the Dispatcher thread.
    /// When using this in your view model you should bind to the CollectionView property in your view model. If you
    /// bind directly this this class it will throw an exception.
    /// 
    /// Note this uses a combination of a list and a dictionary on the back end so the items can be accessed by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ConcurrentObservableDictionary<TKey, TValue> :
    ConcurrentObservableBase<KeyValuePair<TKey, TValue>, ImmutableDictionaryListPair<TKey, TValue>>,
    ICollection<KeyValuePair<TKey, TValue>>,
    IDictionary<TKey, TValue>,
    ICollection,
    ISerializable
    {
        public ConcurrentObservableDictionary() : this(true)
        {
        }

        /// <summary>
        /// Constructructor. Takes an optional isMultithreaded argument where when true allows you to update the collection
        /// from multiple threads. In testing there didn't seem to be any performance hit from turning this on, so I made
        /// it the default.
        /// </summary>
        /// <param name="isThreadSafe"></param>
        public ConcurrentObservableDictionary(bool isMultithreaded) : base(isMultithreaded, ImmutableDictionaryListPair<TKey, TValue>.Empty)
        {
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CollectionView))
                {
                    RaisePropertyChanged(nameof(Keys), nameof(Values));
                }
            };
        }

        public void Add(TKey key, TValue value)
        {
            Add(KeyValuePair.Create(key, value));
        }
        public virtual void Add(KeyValuePair<TKey, TValue> pair)
        {
            DoReadWriteNotify(
              () => _internalCollection.Count,
              (index) => _internalCollection.Add(pair),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair, index)
            );
        }

        /// <summary>
        /// Adds a range of items to the end of the collection. Quicker than adding them individually,
        /// but the view doesn't update until the last item has been added.
        /// </summary>
        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            // Convert to a list off the bat, as this is used multiple times and is required to be
            // an IList for NotifyCollectionChangedEventArgs
            if (!(pairs is IList<KeyValuePair<TKey, TValue>> pairsList))
            {
                pairsList = pairs.ToList();
            }
            DoReadWriteNotify(
              () => _internalCollection.Count,
              (index) => _internalCollection.AddRange(pairsList),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pairsList, index)
            );
        }

        /// <summary>
        /// Adds a value for the key passed in if it isn't already in the dictionary
        /// </summary>
        /// <param name="key">
        /// The object to use as the key of the element to retrieve or add.
        /// </param>
        /// <param name="getValue">
        /// The object factory to use if key doesn't already exist
        /// </param>
        /// <returns>true is new item was added, false otherwise</returns>
        public virtual bool TryAdd(TKey key, Func<TKey, TValue> getValue)
        {
            // Make this nullable so it throws an exception if there's a bug in the code
            KeyValuePair<TKey, TValue>? newPair = null;
            if (DoTestReadWriteNotify(
              // Test if already exists, continue if it doesn't
              () => !_internalCollection.Dictionary.ContainsKey(key),
              // create new node, similar to add
              () => _internalCollection.Count,
              (index) => _internalCollection.Add((newPair = KeyValuePair.Create(key, getValue(key))).Value),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newPair, index)
            ))
            {
                // new one created and added
                return true;
            }
            else
            {
                // key already existed, nothing happened
                return false;
            }
        }

        public bool TryAdd(TKey key, Func<TValue> getValue)
        {
            return TryAdd(key, (keyIn) => getValue());
        }


        /// <summary>
        /// Retrives a value for the key passed in if it exists, else
        /// adds the new value passed in.
        /// </summary>
        /// <param name="key">
        /// The object to use as the key of the element to retrieve or add.
        /// </param>
        /// <param name="getValue">
        /// The object factory to use if key doesn't already exist
        /// </param>
        /// <returns></returns>
        public virtual TValue RetrieveOrAdd(TKey key, Func<TKey, TValue> getValue)
        {
            ObservableDictionaryNode<TKey, TValue> internalNode = null;
            // Make this nullable so it throws an exception if there's a bug in the code
            KeyValuePair<TKey, TValue>? newPair = null;
            if (DoTestReadWriteNotify(
              // Test if already exists, continue if it doesn't
              () => !_internalCollection.Dictionary.TryGetValue(key, out internalNode),
              // create new node, similar to add
              () => _internalCollection.Count,
              (index) => _internalCollection.Add((newPair = KeyValuePair.Create(key, getValue(key))).Value),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newPair, index)
            ))
            {
                // new one created 
                return newPair.Value.Value;
            }
            else
            {
                return internalNode.Value;
            }
        }

        public TValue RetrieveOrAdd(TKey key, Func<TValue> getValue)
        {
            return RetrieveOrAdd(key, (keyIn) => getValue());
        }

        public virtual void Insert(int index, KeyValuePair<TKey, TValue> pair)
        {
            DoWriteNotify(
              () => _internalCollection.Insert(index, pair),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair, index)
            );
        }

        public virtual KeyValuePair<TKey, TValue> RemoveAt(int index)
        {
            KeyValuePair<TKey, TValue> localRemovedItem = default;
            DoReadWriteNotify(
            () => _internalCollection.GetItem(index),
            (item) => _internalCollection.RemoveAt(index),
            (item) =>
            {
                localRemovedItem = item;
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
            }
          );
            return localRemovedItem;
        }

        public bool Remove(TKey key)
        {
            var retVal = DoReadWriteNotify(
              // Get the list of keys and values from the internal list
              () => _internalCollection.GetItemAndIndex(key),
              // remove the keys from the dictionary, remove the range from the list
              (itemAndIndex) => _internalCollection.Remove(key),
              // Notify which items were removed
              (itemAndIndex) => itemAndIndex != null ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemAndIndex.Item, itemAndIndex.Index) : null
            ) != null;
            return retVal;
        }

        public bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            return DoTestReadWriteNotify(
              // Make sure Key/Value pair matches what's in the collection
              () => { var itemAndIndex = _internalCollection.GetItemAndIndex(pair.Key); return itemAndIndex != null && itemAndIndex.Item.Value.Equals(pair.Value); },
              // Get the list of keys and values from the internal list
              () => _internalCollection.GetItemAndIndex(pair.Key),
              // remove the keys from the dictionary, remove the range from the list
              (itemAndIndex) => _internalCollection.Remove(pair.Key),
              // Notify which items were removed
              (itemAndIndex) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemAndIndex.Item, itemAndIndex.Index)
            );
        }

        /// <summary>
        /// Rmoves a range of items by index and count
        /// </summary>
        public IList<KeyValuePair<TKey, TValue>> RemoveRange(int index, int count)
        {
            IList<KeyValuePair<TKey, TValue>> localRemovedItems = null;
            DoReadWriteNotify<IList<KeyValuePair<TKey, TValue>>>(
              // Get the list of keys and values from the internal list
              () => _internalCollection.GetRange(index, count),
              // remove the keys from the dictionary, remove the range from the list
              (items) => _internalCollection.RemoveRange(index, count),
              // Notify which items were removed
              (items) =>
              {
                  localRemovedItems = items;
                  return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index);
              }
            );
            return localRemovedItems;
        }

        public void RemoveRange(IList<TKey> keys)
        {
            DoReadWriteNotify<IList<KeyValuePair<TKey, TValue>>>(
              // Get the list of keys and values from the internal list
              () => _internalCollection.GetRange(keys),
              // remove the keys from the dictionary, remove the range from the list
              (items) => _internalCollection.RemoveRange(keys),
              // Notify which items were removed
              (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items)
            );
        }

        public KeyValuePair<TKey, TValue> GetItem(int index)
        {
            return _internalCollection.GetItem(index);
        }

        /// <summary>
        /// This is the view of the colleciton that you should be binding to with your ListView/GridView control.
        /// </summary>
        public override IList<KeyValuePair<TKey, TValue>> CollectionView
        {
            get
            {
                return ListSelect.Create(_internalCollection.List, node => node.KeyValuePair);
            }
        }

        public IList<TKey> Keys
        {
            get
            {
                return ListSelect.Create(_internalCollection.List, node => node.Key);
            }
        }
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return ListSelect.Create(_internalCollection.List, node => node.Key);
            }
        }

        public IList<TValue> Values
        {
            get
            {
                return ListSelect.Create(_internalCollection.List, node => node.Value);
            }
        }
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return ListSelect.Create(_internalCollection.List, node => node.Value);
            }
        }

        public override int Count
        {
            get
            {
                return _internalCollection.Count;
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                return _internalCollection.Dictionary[key].Value;
            }
            set
            {
                var pair = KeyValuePair.Create(key, value);
                DoTestReadWriteNotify(
                  // Test if adding or replacing  
                  () => !_internalCollection.Dictionary.ContainsKey(key),
                  // Same as Add
                  () => _internalCollection.ItemAndIndexCount,
                  (itemAndIndex) => _internalCollection.Add(pair),
                  (itemAndIndex) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair, itemAndIndex.Index),
                  // Do replace
                  () => _internalCollection.GetItemAndIndex(key),
                  (itemAndIndex) => _internalCollection.ReplaceItem(itemAndIndex.Index, pair),
                  (itemAndIndex) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, pair, itemAndIndex.Item, itemAndIndex.Index));
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{{Items : {Count}}}";
        }



        // ************************************************************************
        // IEnumerable<T> Implementation
        // ************************************************************************
        #region IEnumerable<T> Implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _internalCollection.List.Select(x => x.KeyValuePair).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalCollection.List.Select(x => x.KeyValuePair).GetEnumerator();
        }


        #endregion IEnumerable<T> Implementation

        public bool ContainsKey(TKey key)
        {
            return _internalCollection.Dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var itemAndIndex = _internalCollection.GetItemAndIndex(key);
            if (itemAndIndex != null)
            {
                value = itemAndIndex.Item.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }


        public void Clear()
        {
            DoReadWriteNotify(
              // Get the list of keys and values from the internal list
              () => ListSelect.Create(_internalCollection.List, x => x.KeyValuePair),
              // remove the keys from the dictionary, remove the range from the list
              (items) => ImmutableDictionaryListPair<TKey, TValue>.Empty,
              // Notify which items were removed
              (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0)
            );
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var itemAndIndex = _internalCollection.GetItemAndIndex(item.Key);
            return itemAndIndex != null && itemAndIndex.Item.Value.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            // Take a snapshot of the current list with a converter
            var list = ListSelect.Create(_internalCollection.List, node => node.KeyValuePair);
            list.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            var list = ListSelect.Create(_internalCollection.List, node => node.KeyValuePair);
            ((ICollection)list).CopyTo(array, arrayIndex);
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // ************************************************************************
        // ISerializable Implementation
        // ************************************************************************
        #region ISerializable Implementation
        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            var internalCollection = _internalCollection;
            var children = new KeyValuePair<TKey, TValue>[internalCollection.Count];
            for (int i = 0; i < internalCollection.Count; i++)
                children[i] = internalCollection.GetItem(i);
            info.AddValue("children", children);
        }

        protected ConcurrentObservableDictionary(SerializationInfo information, StreamingContext context) : base(information, context)
        {
            var children = (KeyValuePair<TKey, TValue>[])information.GetValue("children", typeof(KeyValuePair<TKey, TValue>[]));
            _internalCollection = ImmutableDictionaryListPair<TKey, TValue>.Empty.AddRange(children);
        }
        #endregion
    }
}
