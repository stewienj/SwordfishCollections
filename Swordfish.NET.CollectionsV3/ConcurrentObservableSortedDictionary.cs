using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace Swordfish.NET.Collections
{
    [Serializable]
    public class ConcurrentObservableSortedDictionary<TKey, TValue> : ConcurrentObservableDictionary<TKey, TValue>, ISerializable
    {
        private BinarySorter<TKey> _sorter;

        public ConcurrentObservableSortedDictionary() : this(true, null) { }
        public ConcurrentObservableSortedDictionary(bool isMultithreaded) : this(isMultithreaded, null) { }
        public ConcurrentObservableSortedDictionary(IComparer<TKey> comparer) : this(true, comparer) { }
        public ConcurrentObservableSortedDictionary(bool isMultithreaded, IComparer<TKey> comparer) : base(isMultithreaded)
        {
            _sorter = new BinarySorter<TKey>(comparer);
        }

        public override void Add(KeyValuePair<TKey, TValue> pair)
        {
            DoReadWriteNotify(
              () => _sorter.GetInsertIndex(_internalCollection.Count, pair.Key, i => _internalCollection.List[i].Key),
              (index) => _internalCollection.Insert(index, pair),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair, index)
            );
        }

        public override void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            Func<int, ImmutableDictionaryListPair<TKey, TValue>> getIndicesAndInsert = (x) =>
            {
                var updatedCollection = _internalCollection;
                foreach (var pair in pairs)
                {
                    int index = _sorter.GetInsertIndex(updatedCollection.Count, pair.Key, i => updatedCollection.List[i].Key);
                    updatedCollection = updatedCollection.Insert(index, pair);
                }
                return updatedCollection;
            };

            DoReadWriteNotify(
              () => 0,
              getIndicesAndInsert,
              (nothing) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)pairs.ToList())
            );
        }

        public override void Insert(int index, KeyValuePair<TKey, TValue> pair)
        {
            Add(pair);
        }

        public override TValue RetrieveOrAdd(TKey key, Func<TKey, TValue> getValue)
        {
            ObservableDictionaryNode<TKey, TValue> internalNode = null;
            // Make this nullable so it throws an exception if there's a bug in this code
            KeyValuePair<TKey, TValue>? newPair = null;
            if (DoTestReadWriteNotify(
              // Test if already exists, continue if it doesn't
              () => !_internalCollection.Dictionary.TryGetValue(key, out internalNode),
              // create new node, similar to add
              () => _sorter.GetInsertIndex(_internalCollection.Count, key, i => _internalCollection.List[i].Key),
              (index) => _internalCollection.Insert(index, (newPair = KeyValuePair.Create(key, getValue(key))).Value),
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

        public override TValue this[TKey key]
        {
            get
            {
                return base[key];
            }

            set
            {
                var pair = KeyValuePair.Create(key, value);
                DoTestReadWriteNotify(
                  // Test if adding or replacing  
                  () => !_internalCollection.Dictionary.ContainsKey(key),
                  // Similar to Add, but need to match return type arguments
                  () =>
                  {
                      int index = _sorter.GetInsertIndex(_internalCollection.Count, pair.Key, i => _internalCollection.List[i].Key);
                      return new ImmutableDictionaryListPair<TKey, TValue>.ItemAndIndex(pair, index);
                  },
                  (itemAndIndex) => _internalCollection.Insert(itemAndIndex.Index, pair),
                  (itemAndIndex) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair, itemAndIndex.Index),
                  // Do replace
                  () => _internalCollection.GetItemAndIndex(key),
                  (itemAndIndex) => _internalCollection.ReplaceItem(itemAndIndex.Index, pair),
                  (itemAndIndex) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, pair, itemAndIndex.Item, itemAndIndex.Index));
            }
        }

        // ************************************************************************
        // ISerializable Implementation
        // ************************************************************************
        #region ISerializable Implementation
        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected ConcurrentObservableSortedDictionary(SerializationInfo information, StreamingContext context) : base(information, context)
        {

        }
        #endregion
    }
}


