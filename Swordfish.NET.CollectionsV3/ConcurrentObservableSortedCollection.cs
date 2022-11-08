using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;

namespace Swordfish.NET.Collections
{
    public class ConcurrentObservableSortedCollection<T> : ConcurrentObservableCollection<T>
    {
        private BinarySorter<T> _sorter;

        public ConcurrentObservableSortedCollection() : this(true, null, null) { }
        public ConcurrentObservableSortedCollection(bool isMultithreaded) : this(isMultithreaded, null, null) { }
        public ConcurrentObservableSortedCollection(IEnumerable<T> source) : this(true, source, null) { }
        public ConcurrentObservableSortedCollection(IComparer<T> comparer) : this(true, null, comparer) { }
        public ConcurrentObservableSortedCollection(IEnumerable<T> source, IComparer<T> comparer) : this(true, source, comparer) { }
        public ConcurrentObservableSortedCollection(bool isMultithreaded, IComparer<T> comparer) : this(isMultithreaded, null, comparer) { }
        public ConcurrentObservableSortedCollection(bool isMultithreaded, IEnumerable<T> source, IComparer<T> comparer) : base(isMultithreaded)
        {
            _sorter = new BinarySorter<T>(comparer);
            if (source is IList<T> list)
            {
                AddRange(list);
            }
            else if (source != null)
            {
                AddRange(source.ToList());
            }
        }

        protected override int IListAdd(T item)
        {
            return DoReadWriteNotify(
              () => _sorter.GetInsertIndex(ImmutableList.Count, item, i => _internalCollection[i]),
              (index) => ImmutableList.Insert(index, item),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
            );
        }

        public override void AddRange(IList<T> items)
        {
            Func<int, IList<T>> getIndicesAndInsert = (x) =>
            {
                var updatedCollection = ImmutableList;
                foreach (var item in items)
                {
                    int index = _sorter.GetInsertIndex(updatedCollection.Count, item, i => updatedCollection[i]);
                    updatedCollection = updatedCollection.Insert(index, item);
                }
                return updatedCollection;
            };

            DoReadWriteNotify(
              () => 0,
              getIndicesAndInsert,
              (nothing) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items.ToList())
            );
        }

        public override void Reset(IList<T> items) =>
            DoReadWriteNotify(
              // Sort the incoming collection and add it directly to the internal collection.
              // Should be quicker than sorting 1 by 1 on insert.
              () => ImmutableList.ToArray(),
              (oldItems) => ImmutableList<T>.Empty.AddRange(items.OrderBy(x => x, _sorter).ToList()),
              (oldItems) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)oldItems, 0),
              (oldItems) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, 0)
            );

        public override void Insert(int index, T item)
        {
            Add(item);
        }

        public override void InsertRange(int index, IList<T> items)
        {
            AddRange(items);
        }

        public override T this[int index]
        {
            get=> base[index];
            set
            {
                RemoveAt(index);
                Add(value);
            }
        }
    }
}


