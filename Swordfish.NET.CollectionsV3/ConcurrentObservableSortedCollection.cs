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

        public ConcurrentObservableSortedCollection() : this(true, null) { }
        public ConcurrentObservableSortedCollection(bool isMultithreaded) : this(isMultithreaded, null) { }
        public ConcurrentObservableSortedCollection(IComparer<T> comparer) : this(true, comparer) { }
        public ConcurrentObservableSortedCollection(bool isMultithreaded, IComparer<T> comparer) : base(isMultithreaded)
        {
            _sorter = new BinarySorter<T>(comparer);
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
              () => new OldAndNew(ImmutableList.ToArray(),items.OrderBy(x => x, _sorter).ToList()),
              (oldAndNew) => ImmutableList<T>.Empty.AddRange(oldAndNew.New),
              (oldAndNew) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)oldAndNew.Old, 0),
              (oldAndNew) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)oldAndNew.New, 0)
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

        private class OldAndNew {
            public OldAndNew(T[] old, List<T> @new)
            {
                Old = old;
                New = @new;
            }
            public T[] Old;
            public List<T> New;
        }
    }
}


