using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ConcurrentObservableDictionaryLite<TKey, TValue> :
    ConcurrentObservableBase<KeyValuePair<TKey, TValue>, ImmutableDictionary<TKey, TValue>>,
    ICollection<KeyValuePair<TKey, TValue>>,
    IDictionary<TKey, TValue>,
    ICollection,
    ISerializable
    {
        public ConcurrentObservableDictionaryLite() : this(true)
        {
        }

        public ConcurrentObservableDictionaryLite(IEqualityComparer<TKey> keyComparer) : base(true, ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer))
        {
        }

        /// <summary>
        /// Constructructor. Takes an optional isMultithreaded argument where when true allows you to update the collection
        /// from multiple threads. In testing there didn't seem to be any performance hit from turning this on, so I made
        /// it the default.
        /// </summary>
        /// <param name="isThreadSafe"></param>
        public ConcurrentObservableDictionaryLite(bool isMultithreaded) : base(isMultithreaded, ImmutableDictionary<TKey, TValue>.Empty)
        {
        }

        public void Add(TKey key, TValue value)
        {
            DoWriteNotify(
              () =>_internalCollection.Add(key, value),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value))
            );
        }

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Adds a range of items to the end of the collection. Quicker than adding them individually,
        /// but the view doesn't update until the last item has been added.
        /// </summary>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            // Convert to a list off the bat, as this is used multiple times and is required to be
            // an IList for NotifyCollectionChangedEventArgs
            var valuesList = values as IList<KeyValuePair<TKey, TValue>> ?? values.ToList();

            DoWriteNotify(
              () => _internalCollection.AddRange(valuesList),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)valuesList)
            );
        }

        public bool Remove(TKey value)
        {
            bool wasRemoved = false;
            DoWriteNotify(
              () =>
              {
                  var newCollection = _internalCollection.Remove(value);
                  wasRemoved = newCollection != _internalCollection;
                  return newCollection;
              },
              () => wasRemoved ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value) : null
            );
            return wasRemoved;
        }

        public void RemoveRange(IEnumerable<TKey> keys)
        {
            // Convert to a list off the bat, as this is used multiple times and is required to be
            // an IList for NotifyCollectionChangedEventArgs
            var keysList = keys as IList<TKey> ?? keys.ToList();

            DoWriteNotify(
              () => _internalCollection.RemoveRange(keysList),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)keysList)
            );
        }

        public bool TryRemove(TKey key, out TValue item)
        {
            TValue tempItem = default(TValue);
            var removed = DoReadWriteNotify(
              // Get the list of keys and values from the internal list
              () => _internalCollection.TryGetValue(key, out tempItem),
              // remove the keys from the dictionary, remove the range from the list
              (found) => found ? _internalCollection.Remove(key) : _internalCollection,
              // Notify which items were removed
              (found) => found ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, key) : null
            );

            item = tempItem;
            return removed;
        }

        /// <summary>
        /// This is the view of the colleciton that you should be binding to with your ListView/GridView control.
        /// </summary>
        public override IList<KeyValuePair<TKey, TValue>> CollectionView => _internalCollection.ToList();

        public override int Count => _internalCollection.Count;

        public bool IsReadOnly => false;

        public override string ToString()
        {
            return $"{{Items : {Count}}}";
        }

        public void Clear()
        {
            DoReadWriteNotify(
              // Get the list of keys and values from the internal list
              () => _internalCollection.ToList(),
              // remove the keys from the dictionary, remove the range from the list
              (items) => ImmutableDictionary<TKey, TValue>.Empty,
              // Notify which items were removed
              (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items, 0)
            );
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _internalCollection.Contains(item);

        public bool ContainsKey(TKey key) => _internalCollection.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_internalCollection).CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int arrayIndex) => ((ICollection)_internalCollection).CopyTo(array, arrayIndex);

        object ICollection.SyncRoot => ((ICollection)_internalCollection).SyncRoot;

        bool ICollection.IsSynchronized => ((ICollection)_internalCollection).IsSynchronized;

        public ICollection<TKey> Keys => _internalCollection.Keys.ToList();

        public ICollection<TValue> Values => _internalCollection.Values.ToList();

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => throw new NotImplementedException();

        public TValue this[TKey key]
        {
            get => _internalCollection[key];
            set
            {
                var pair = KeyValuePair.Create(key, value);
                DoTestReadWriteNotify(
                  // Test if adding or replacing  
                  () => !_internalCollection.ContainsKey(key),
                  // Same as Add
                  () => (KeyValuePair<TKey, TValue>?)null,
                  (_) => _internalCollection.Add(key, value),
                  (_) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair),
                  // Do replace
                  () => new KeyValuePair<TKey, TValue>(key, _internalCollection[key]),
                  (oldPair) => _internalCollection.SetItem(key, value),
                  (oldPair) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, pair, oldPair));
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _internalCollection.TryGetValue(key, out value);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_internalCollection.TryGetValue(item.Key, out var value))
            {
                // Check that value matches
                if (value.Equals(item.Value))
                {
                    return Remove(item.Key);
                }
            }
            return false;
        }

        // ************************************************************************
        // IEnumerable<T> Implementation
        // ************************************************************************
        #region IEnumerable<KeyValuePair<TKey, TValue>> Implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }

        #endregion IEnumerable<KeyValuePair<TKey, TValue>> Implementation

        // ************************************************************************
        // ISerializable Implementation
        // ************************************************************************
        #region ISerializable Implementation
        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            var children = _internalCollection.ToArray();
            info.AddValue("children", children);
        }

        protected ConcurrentObservableDictionaryLite(SerializationInfo information, StreamingContext context) : base(information, context)
        {
            var children = (KeyValuePair<TKey, TValue>[])information.GetValue("children", typeof(KeyValuePair<TKey, TValue>[]));
            _internalCollection = ImmutableDictionary<TKey, TValue>.Empty.AddRange(children);
        }
        #endregion
    }
}
