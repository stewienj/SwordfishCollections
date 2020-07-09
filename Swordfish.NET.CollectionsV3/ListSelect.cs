using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Swordfish.NET.Collections
{
    /// <summary>
    /// Factory for creating ListSelect<TSource, TResult> objects
    /// </summary>
    public static class ListSelect
    {
        public static ListSelect<TSource, TResult> Create<TSource, TResult>(IList<TSource> sourceList, Func<TSource, TResult> select)
        {
            return new ListSelect<TSource, TResult>(sourceList, select);
        }
    }
    /// <summary>
    /// Provides an IList interface that lets you use a list with a select to type convert a list as its being accessed
    /// </summary>
    public class ListSelect<TSource, TResult> : IList<TResult>
    {
        private IList<TSource> _sourceList;
        private Func<TSource, TResult> _select;


        public ListSelect(IList<TSource> sourceList, Func<TSource, TResult> select)
        {
            _sourceList = sourceList;
            _select = select;
        }

        TResult IList<TResult>.this[int index]
        {
            get
            {
                return _select(_sourceList[index]);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        int ICollection<TResult>.Count
        {
            get
            {
                return _sourceList.Count;
            }
        }

        bool ICollection<TResult>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public void Add(TResult item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TResult item)
        {
            return _sourceList.Select(x => _select(x)).Where(x => x.Equals(item)).Any();
        }

        public void CopyTo(TResult[] array, int arrayIndex)
        {
            int count = _sourceList.Count;

            for (int destIndex = arrayIndex, sourceIndex = 0; sourceIndex < count; ++destIndex, ++sourceIndex)
            {
                array[destIndex] = _select(_sourceList[sourceIndex]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sourceList.Select(x => _select(x)).GetEnumerator();
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return _sourceList.Select(x => _select(x)).GetEnumerator();
        }

        public int IndexOf(TResult item)
        {
            int index = 0;
            foreach (var sourceItem in this)
            {
                if (item.Equals(sourceItem))
                {
                    return index;
                }
                ++index;
            }
            return -1;
        }

        void IList<TResult>.Insert(int index, TResult item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<TResult>.Remove(TResult item)
        {
            throw new NotImplementedException();
        }

        void IList<TResult>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
