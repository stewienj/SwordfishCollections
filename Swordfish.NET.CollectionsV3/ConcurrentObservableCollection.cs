using Swordfish.NET.Collections.EditableBridges;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections
{
    public interface IConcurrentObservableList<T> : IConcurrentObservableBase<T>, IList<T>, IList
    {
        ImmutableList<T> ImmutableList { get; }
    };

    /// <summary>
    /// A collection that can be updated from multiple threads, and can be bound to an items control in the user interface.
    /// Has the advantage over ObservableCollection in that it doesn't have to be updated from the Dispatcher thread.
    /// When using this in your view model you should bind to the CollectionView property in your view model. If you
    /// bind directly this this class it will throw an exception.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ConcurrentObservableCollection<T> :
    // Use IList<T> as the internal collection type parameter, not ImmutableList
    // otherwise everything that uses this needs to reference the corresponding assembly
    ConcurrentObservableBase<T, IList<T>>,
    IConcurrentObservableList<T>,
    IEditableCollection,
    IList<T>,
    IList,
    ISerializable
    {
        /// <summary>
        /// The CollectionView, used in Binding to a ListView or DataGrid is immutable, thus readonly,
        /// this bridge passes the writes from the CollectionView back to the underlying collection,
        /// and handles some syncronization issues.
        /// </summary>
        private EditableImmutableListBridge<T> _editableCollectionView;

        // ********************************************************************
        // Constructors
        // ********************************************************************
        #region Constructors

        public ConcurrentObservableCollection() : this(true)
        {
        }

        public ConcurrentObservableCollection(IEnumerable<T> source) : this(true)
        {
            if (source is IList<T> list)
            {
                AddRange(list);
            }
            else
            {
                AddRange(source.ToList());
            }
        }

        /// <summary>
        /// Constructructor. Takes an optional isMultithreaded argument where when true allows you to update the collection
        /// from multiple threads. In testing there didn't seem to be any performance hit from turning this on, so I made
        /// it the default.
        /// </summary>
        /// <param name="isThreadSafe"></param>
        public ConcurrentObservableCollection(bool isMultithreaded) : base(isMultithreaded, ImmutableList<T>.Empty)
        {
            _editableCollectionView = EditableImmutableListBridge<T>.Empty(this);
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CollectionView))
                {
                    RaisePropertyChanged(nameof(EditableCollectionView));
                }
            };
        }

        #endregion Constructors

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs changes)
        {
            // Assign new value to CollectionView so it is recognised as different to existing value
            _editableCollectionView = _editableCollectionView.UpdateSource((ImmutableList<T>)_internalCollection);
            base.OnCollectionChanged(changes);
        }

        public ImmutableList<T> ImmutableList => (ImmutableList<T>)_internalCollection;

        protected virtual int IListAdd(T item) =>
            DoReadWriteNotify(
              () => ImmutableList.Count,
              (index) => ImmutableList.Add(item),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
            );

        public T RemoveLast() =>
            DoReadWriteNotify(
              () => new { Index = ImmutableList.Count - 1, Item = ImmutableList.LastOrDefault() },
              (indexAndItem) => indexAndItem.Index < 0 ? ImmutableList : ImmutableList.RemoveAt(indexAndItem.Index),
              (indexAndItem) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, indexAndItem.Item)
            ).Item;

        /// <summary>
        /// Adds a range of items to the end of the collection. Quicker than adding them individually,
        /// but the view doesn't update until the last item has been added.
        /// </summary>
        public virtual void AddRange(IList<T> items) =>
            DoReadWriteNotify(
              () => ImmutableList.Count,
              (index) => ImmutableList.AddRange(items),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items as IList ?? items.ToList(), index)
            );

        /// <summary>
        /// Inserts a range of items at the position specified. *Much quicker* than adding them
        /// individually, but the view doesn't update until the last item has been inserted.
        /// </summary>
        public virtual void InsertRange(int index, IList<T> items) =>
            DoWriteNotify(
              () => ImmutableList.InsertRange(index, items),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items as IList ?? items.ToList(), index)
            );

        /// <summary>
        /// Rmoves a range of items by index and count
        /// </summary>
        public void RemoveRange(int index, int count) =>
            DoReadWriteNotify(
              () => ImmutableList.GetRange(index, count),
              (items) => ImmutableList.RemoveRange(index, count),
              (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index)
            );

        public void RemoveRange(IList<T> items) =>
            DoWriteNotify(
              () => ImmutableList.RemoveRange(items),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items as IList ?? items.ToList())
            );

        public virtual void Reset(IList<T> items) =>
            DoReadWriteNotify(
              () => ImmutableList.ToArray(),
              (oldItems) => ImmutableList<T>.Empty.AddRange(items),
              (oldItems) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, 0),
              (oldItems) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items as IList ?? items.ToList(), 0)
            );

        public T[] ToArray() => ImmutableList.ToArray();

        public List<T> ToList() => ImmutableList.ToList();

        public override string ToString() => $"{{Items : {Count}}}";

        // ************************************************************************
        // EditableCollection region. Methods and Properties for handling editing
        // the collection in a DataGrid. 
        // ************************************************************************
        #region EditableCollection

        /// <summary>
        /// Flags that an item is being edited, this should be called from the GUI thread.
        /// This stops updates from going through that would cause a data grid to exit
        /// editing mode.
        ///
        /// This would normally be called from a WPF DataGrid BeginningEdit event.
        /// </summary>
        public void BeginEditingItem()
        {
            _lock.EnterWriteLock();
            _editableCollectionView.FreezeUpdates = true;
            _lock.ExitWriteLock();
        }

        /// <summary>
        /// Clears flag for indicating an item is being edited, this should be called from the GUI thread,
        /// and called from the CurrentCellChanged event, as the CellEditEnding event gets fired before the
        /// edit is committed and will be lost when the underlying collection gets refreshed.
        ///
        /// This would normally be called from a WPF DataGrid CurrentCellChanged event.
        /// </summary>
        public void EndedEditingItem()
        {
            // Clear flag
            _lock.EnterWriteLock();
            _editableCollectionView.FreezeUpdates = false;
            _lock.ExitWriteLock();
        }

        /// <summary>
        /// Bind to this property in the ItemSource of a WPF DataGrid to allow editing
        /// of the collection from the view. I could have made the CollectionView property
        /// editable, but I didn't want to change the item that was returned as it might
        /// have broken someones code that depended on the return type being an immutable
        /// collection.
        /// </summary>
        public IList<T> EditableCollectionView => _editableCollectionView;

        #endregion

        // ************************************************************************
        // IEnumerable<T> Implementation
        // ************************************************************************
        #region IEnumerable<T> Implementation

        public IEnumerator<T> GetEnumerator() => ImmutableList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ImmutableList.GetEnumerator();

        #endregion IEnumerable<T> Implementation

        // ************************************************************************
        // IList<T> Implementation
        // ************************************************************************
        #region IList<T> Implementation

        public int IndexOf(T item) => ImmutableList.IndexOf(item);

        public virtual void Insert(int index, T item) =>
            DoWriteNotify(
              () => ImmutableList.Insert(index, item),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
            );

        public void RemoveAt(int index) =>
            DoReadWriteNotify(
              () => ImmutableList[index],
              (item) => ImmutableList.RemoveAt(index),
              (item) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index)
            );

        public virtual T this[int index]
        {
            get => ImmutableList[index];
            set =>
                DoReadWriteNotify(
                  () => ImmutableList[index],
                  (item) => ImmutableList.SetItem(index, value),
                  (item) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item, index)
                );
        }

        #endregion IList<T> Implementation

        // ************************************************************************
        // ICollection<T> Implementation
        // ************************************************************************
        #region ICollection<T> Implementation

        public void Add(T item) => IListAdd(item);

        public void Clear() =>
            DoReadWriteNotify(
              () => ImmutableList.ToArray(),
              (items) => ImmutableList.Clear(),
              (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0)
            );

        public bool Contains(T item) => ImmutableList.Contains(item);

        // There's some interplay between Count and CopyTo where an array/list is created
        // and copied to. Unfortunately the collection can change between these two calls.
        // So we take a snapshot that can be used to copy from.
        private ConcurrentDictionary<Thread, ImmutableList<T>> _collectionAtlastCount = new ConcurrentDictionary<Thread, ImmutableList<T>>();
        int ICollection<T>.Count
        {
            get
            {
                var list = ImmutableList;
                _collectionAtlastCount[Thread.CurrentThread] = list;
                return list.Count;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ImmutableList<T> oldList = null;
            if (!_collectionAtlastCount.TryRemove(Thread.CurrentThread, out oldList))
            {
                oldList = ImmutableList;
            }
            int copyCount = Math.Min(array.Length - arrayIndex, oldList.Count);
            for (int i = 0; i < copyCount; ++i)
            {
                array[i + arrayIndex] = oldList[i];
            }
        }

        public override int Count => ImmutableList.Count;

        public bool IsReadOnly => false;

        public override IList<T> CollectionView => _internalCollection;

        public bool Remove(T item) =>
            DoReadWriteNotify(
              () => ImmutableList.IndexOf(item),
              (index) => index < 0 ? ImmutableList : ImmutableList.RemoveAt(index),
              (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index)
            ) >= 0;

        #endregion ICollection<T> Implementation

        // ************************************************************************
        // ICollection Implementation
        // ************************************************************************
        #region ICollection Implementation

        void ICollection.CopyTo(Array array, int index) => ((ICollection)ImmutableList).CopyTo(array, index);

        bool ICollection.IsSynchronized => ((ICollection)ImmutableList).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)ImmutableList).SyncRoot;

        #endregion ICollection Implementation

        // ************************************************************************
        // IList Implementation
        // ************************************************************************
        #region IList Implementation

        int IList.Add(object value) =>
            (value is T item) ?
                IListAdd(item) :
                -1;

        bool IList.Contains(object value) => ((IList)ImmutableList).Contains(value);

        int IList.IndexOf(object value) => ((IList)ImmutableList).IndexOf(value);

        void IList.Insert(int index, object value) => Insert(index, (T)value);

        bool IList.IsFixedSize => ((IList)ImmutableList).IsFixedSize;

        bool IList.IsReadOnly => ((IList)ImmutableList).IsReadOnly;

        void IList.Remove(object value) => Remove((T)value);

        void IList.RemoveAt(int index) => RemoveAt(index);

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        #endregion IList Implementation

        // ************************************************************************
        // ISerializable Implementation
        // ************************************************************************
        #region ISerializable Implementation

        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("children", _internalCollection.ToArray());
        }

        protected ConcurrentObservableCollection(SerializationInfo information, StreamingContext context) : base(information, context)
        {
            _internalCollection = System.Collections.Immutable.ImmutableList.CreateRange((T[])information.GetValue("children", typeof(T[])));
        }

        #endregion ISerializable Implementation
    }
}
