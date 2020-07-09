using System.Collections.Generic;

namespace Swordfish.NET.Collections
{
    public class SortedListWithDuplicates<T> : IList<T>
    {
        private IList<T> _list;
        private BinarySorter<T> _sorter;

        /// <summary>
        /// Constructor. Lets you pass in the underlying list...which needs to be used with care
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="baseList"></param>
        public SortedListWithDuplicates(IComparer<T> comparer = null, IList<T> baseList = null)
        {
            _sorter = new BinarySorter<T>(comparer);
            _list = baseList ?? new List<T>();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Add(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                RemoveAt(index);
                Add(value);
            }
        }

        public void Add(T item)
        {
            int index = _sorter.GetInsertIndex(_list.Count, item, i => _list[i]);
            _list.Insert(index, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_list).GetEnumerator();
        }
    }
}
