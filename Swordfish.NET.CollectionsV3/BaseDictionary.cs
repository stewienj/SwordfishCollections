using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Swordfish.NET.Collections
{
    /// <summary>
    /// Represents a dictionary mapping keys to values.
    /// </summary>
    /// 
    /// <remarks>
    /// Provides the plumbing for the portions of IDictionary<TKey,
    /// TValue> which can reasonably be implemented without any
    /// dependency on the underlying representation of the dictionary.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    public abstract class BaseDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private KeyCollection _keys;
        private ValueCollection _values;

        protected BaseDictionary() { }

        public abstract int Count { get; }
        public abstract void Clear();
        public abstract void Add(TKey key, TValue value);
        public abstract bool ContainsKey(TKey key);
        public abstract bool Remove(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);
        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        protected abstract void SetValue(TKey key, TValue value);

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => (_keys = _keys ?? new KeyCollection(this));

        public ICollection<TValue> Values => (_values = _values ?? new ValueCollection(this));

        public TValue this[TKey key]
        {
            get => TryGetValue(key, out TValue value) ?
                    value :
                    throw new KeyNotFoundException();
            set => SetValue(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out TValue value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Copy(this, array, arrayIndex);

        public bool Remove(KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private abstract class Collection<T> : ICollection<T>
        {
            protected readonly IDictionary<TKey, TValue> _dictionary;

            protected Collection(IDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

            public int Count => _dictionary.Count;

            public bool IsReadOnly => true;

            public void CopyTo(T[] array, int arrayIndex) => Copy(this, array, arrayIndex);

            public virtual bool Contains(T item) => this.Any(element => EqualityComparer<T>.Default.Equals(element, item));

            public IEnumerator<T> GetEnumerator()
            {
                foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
                    yield return GetItem(pair);
            }

            protected abstract T GetItem(KeyValuePair<TKey, TValue> pair);

            public bool Remove(T item) => throw new NotSupportedException("Collection is read - only.");

            public void Add(T item) => throw new NotSupportedException("Collection is read - only.");

            public void Clear() => throw new NotSupportedException("Collection is read - only.");

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [DebuggerDisplay("Count = {Count}")]
        private class KeyCollection : Collection<TKey>
        {
            public KeyCollection(IDictionary<TKey, TValue> dictionary)
                : base(dictionary) { }

            protected override TKey GetItem(KeyValuePair<TKey, TValue> pair) => pair.Key;
            public override bool Contains(TKey item) => _dictionary.ContainsKey(item);
        }

        [DebuggerDisplay("Count = {Count}")]
        private class ValueCollection : Collection<TValue>
        {
            public ValueCollection(IDictionary<TKey, TValue> dictionary)
                : base(dictionary) { }

            protected override TValue GetItem(KeyValuePair<TKey, TValue> pair) => pair.Value;
        }

        private static void Copy<T>(ICollection<T> source, T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if ((array.Length - arrayIndex) < source.Count)
                throw new ArgumentException("Destination array is not large enough.Check array.Length and arrayIndex.");

            foreach (T item in source)
                array[arrayIndex++] = item;
        }
    }
}
