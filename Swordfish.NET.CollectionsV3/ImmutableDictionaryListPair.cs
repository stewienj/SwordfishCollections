using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Swordfish.NET.Collections
{

    public class ImmutableDictionaryListPair<TKey, TValue>
    {
        /// <summary>
        /// Class holds the item and the index of the item. Used as an intermediate when performing ops on the collections.
        /// </summary>
        public class ItemAndIndex
        {
            public ItemAndIndex(KeyValuePair<TKey, TValue> item, int index)
            {
                Item = item;
                Index = index;
            }

            public KeyValuePair<TKey, TValue> Item { get; }
            public int Index { get; }
        }

        private BinarySorter<BigRationalOld> _indexFinder = new BinarySorter<BigRationalOld>();

        /// <summary>
        /// Note using a class to wrap KeyValuePair (which is a value type), so the exact object can correctly be removed by key in the instance that two or more values are the same
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="list"></param>
        internal ImmutableDictionaryListPair(ImmutableDictionary<TKey, ObservableDictionaryNode<TKey, TValue>> dictionary, ImmutableList<ObservableDictionaryNode<TKey, TValue>> list)
        {
            Dictionary = dictionary;
            List = list;
        }

        public ImmutableDictionaryListPair<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer)
        {
            return new ImmutableDictionaryListPair<TKey, TValue>(
                dictionary: Dictionary.WithComparers(keyComparer: keyComparer), list: List);
        }

        public ImmutableDictionaryListPair<TKey, TValue> Add(KeyValuePair<TKey, TValue> pair)
        {
            var endNode = List.Any() ? List[List.Count - 1] : null;

            // Create the list of nodes for the internal list
            var node = new ObservableDictionaryNode<TKey, TValue>(pair, endNode);

            // create the new immutable Dictionary/List pair
            return new ImmutableDictionaryListPair<TKey, TValue>(Dictionary.Add(pair.Key, node), List.Add(node));
        }

        public ImmutableDictionaryListPair<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            var endNode = List.Any() ? List[List.Count - 1] : null;

            // Create the list of nodes for the internal list
            var nodes = pairs.SelectWithPreviousResult(
              firstItem => new ObservableDictionaryNode<TKey, TValue>(firstItem, endNode),
              (previousNode, pair) => new ObservableDictionaryNode<TKey, TValue>(pair, previousNode))
                .ToList();

            // create the key/value pairs for the internal dictionary
            var dictionaryEntries = nodes.Select(node => KeyValuePair.Create(node.Key, node));

            // create the new immutable Dictionary/List pair
            return new ImmutableDictionaryListPair<TKey, TValue>(Dictionary.AddRange(dictionaryEntries), List.AddRange(nodes));
        }

        public ImmutableDictionaryListPair<TKey, TValue> Insert(int index, KeyValuePair<TKey, TValue> pair)
        {
            if (index == Count)
            {
                return Add(pair);
            }
            else if (index > Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Needs to be greated than -1 and less than the count of {Count}");
            }

            // Need to find the node position numbers either side, take the average, and that is the new sort key
            BigRationalOld insertSortKey = index > 0 ?
              (List[index - 1].SortKey + List[index].SortKey) / (BigRationalOld)2 :
              List[index].SortKey - BigRationalOld.One;


            // Create the list of nodes for the internal list
            var node = new ObservableDictionaryNode<TKey, TValue>(pair, insertSortKey);
            // Double update the lists inserting the new node and replacing the after node with a modified version
            var newList = List.Insert(index, node);
            var newDictionary = Dictionary.Add(node.Key, node);
            // Create the new collection
            return new ImmutableDictionaryListPair<TKey, TValue>(newDictionary, newList);
        }


        public IList<KeyValuePair<TKey, TValue>> GetRange(int index, int count)
        {
            var subList = List.GetRange(index, count);
            return ListSelect.Create(subList, (s) => s.KeyValuePair);
        }

        /// <summary>
        /// Gets the range of key value pairs for the keys that are in both the dictionary and the list passed in
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IList<KeyValuePair<TKey, TValue>> GetRange(IList<TKey> keys)
        {
            var subList = keys.Where(key => Dictionary.ContainsKey(key)).Select(key => Dictionary[key]).Select(node => node.KeyValuePair).ToList();
            return subList;
        }

        /// <summary>
        /// Removes the dictionary if the key passed in is contained therein
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ImmutableDictionaryListPair<TKey, TValue> Remove(TKey key)
        {
            ObservableDictionaryNode<TKey, TValue> nodeToRemove;
            if (Dictionary.TryGetValue(key, out nodeToRemove))
            {
                int indexToRemoveAt = GetIndex(nodeToRemove);
                return new ImmutableDictionaryListPair<TKey, TValue>(Dictionary.Remove(key), List.RemoveAt(indexToRemoveAt));
            }
            else
            {
                return this;
            }
        }

        public ImmutableDictionaryListPair<TKey, TValue> RemoveAt(int index)
        {
            ObservableDictionaryNode<TKey, TValue> nodeToRemove = List[index];
            return new ImmutableDictionaryListPair<TKey, TValue>(Dictionary.Remove(nodeToRemove.Key), List.RemoveAt(index));
        }


        public ImmutableDictionaryListPair<TKey, TValue> RemoveRange(int index, int count)
        {
            var keysToRemove = List.GetRange(index, count).Select(x => x.Key);
            return new ImmutableDictionaryListPair<TKey, TValue>(Dictionary.RemoveRange(keysToRemove), List.RemoveRange(index, count));
        }

        public ImmutableDictionaryListPair<TKey, TValue> RemoveRange(IList<TKey> keys)
        {
            var nodesToRemove = keys.Where(key => Dictionary.ContainsKey(key)).Select(key => Dictionary[key]);
            if (nodesToRemove.Any())
            {
                var newDictionary = Dictionary.RemoveRange(keys);
                var newList = List.RemoveRange(nodesToRemove);
                return new ImmutableDictionaryListPair<TKey, TValue>(newDictionary, newList);
            }

            else
            {
                return this;
            }
        }

        public KeyValuePair<TKey, TValue> GetItem(int index)
        {
            return List[index].KeyValuePair;
        }

        public ItemAndIndex GetItemAndIndex(TKey key)
        {
            if (Dictionary.ContainsKey(key))
            {
                var node = Dictionary[key];
                //var index = List.IndexOf(node);
                var index = GetIndex(node);
                return new ItemAndIndex(node.KeyValuePair, index);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the index of the key passed in. If the index can't be determined then -1 is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int GetIndex(ObservableDictionaryNode<TKey, TValue> node)
        {
            int matchIndex = _indexFinder.GetMatchIndex(List.Count, node.SortKey, index => List[index].SortKey);
            if (matchIndex < 0 && !node.Equals(List[matchIndex]))
            {
                throw new InvalidOperationException("Theres a bug in the code that finds the dictionary index");
            }

            return matchIndex;
        }

        public ImmutableDictionaryListPair<TKey, TValue> ReplaceItem(int index, KeyValuePair<TKey, TValue> pair)
        {
            ObservableDictionaryNode<TKey, TValue> node = new ObservableDictionaryNode<TKey, TValue>(pair, List[index].SortKey);
            return new ImmutableDictionaryListPair<TKey, TValue>(Dictionary.SetItem(node.Key, node), List.SetItem(index, node));
        }

        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        public ItemAndIndex ItemAndIndexCount
        {
            get
            {
                return new ItemAndIndex(default(KeyValuePair<TKey, TValue>), List.Count);
            }
        }


        internal ImmutableDictionary<TKey, ObservableDictionaryNode<TKey, TValue>> Dictionary { get; }
        internal ImmutableList<ObservableDictionaryNode<TKey, TValue>> List { get; }
        public static ImmutableDictionaryListPair<TKey, TValue> Empty { get; } = new ImmutableDictionaryListPair<TKey, TValue>(ImmutableDictionary<TKey, ObservableDictionaryNode<TKey, TValue>>.Empty, ImmutableList<ObservableDictionaryNode<TKey, TValue>>.Empty);

    }
}
