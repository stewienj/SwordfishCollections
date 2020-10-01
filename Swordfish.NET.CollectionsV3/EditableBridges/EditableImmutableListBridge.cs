using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;

namespace Swordfish.NET.Collections.EditableBridges
{
    /// <summary>
    /// In the ConcurrentObservableXXXX collections contained in this library there is a property
    /// called CollectionView that is bound to a WPF list control or data grid. This CollectionView
    /// is an immutable collection, necessary as you can't have the collection changing from a non
    /// GUI thread, so instead we take immutable snapshots of the underlying collection. This is fine
    /// until you bind to a control like DataGrid which allows for modification of the collection.
    /// 
    /// This class bridges the CollectionView back to the writable collection. So all reads come from the
    /// immutable collection (CollectionView), and all writes are directed to the original writable collection.
    /// 
    /// If that was all that was done there might be a timing issue where the underlying collection is
    /// different to what the GUI thinks it is as it hasn't been updated before the GUI uses it. To get
    /// around this issue the underlying source is updated immediately when updates are made through
    /// this bridge.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class EditableImmutableListBridge<T> : IList<T>, IList // Have to implement IList to get the new row in WPF DataGrid
    {
        /// <summary>
        /// This is modifyable concurrent observable collection, used as a destination for changes
        /// </summary>
        private IConcurrentObservableList<T> _destination;
        /// <summary>
        /// An immutable collection, used for collection reads
        /// </summary>
        private ImmutableList<T> _source;
        /// <summary>
        /// Used to keep track of when we are inside of an edit so we can detect
        /// re-entrance into this class.
        /// </summary>
        private volatile Thread _guiCallerThread = null;

        private EditableImmutableListBridge(ImmutableList<T> source, IConcurrentObservableList<T> destination)
        {
            _source = source;
            _destination = destination;

            Type type = typeof(EditableImmutableListBridge<T>);
            bool isGeneric = type.IsGenericType;
            Type[] genericArguments = type.GetGenericArguments();
            int length = genericArguments.Length;
        }

        /// <summary>
        /// Just using immutable collection style of getting an empty collection because these
        /// collection classes are based on immutable collections.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static EditableImmutableListBridge<T> Empty(IConcurrentObservableList<T> destination) => new EditableImmutableListBridge<T>(ImmutableList<T>.Empty, destination);

        /// <summary>
        /// This gets a new collection to be presented to the view, unless the current change
        /// is being performed in this object, where all changes should only be coming from 
        /// the GUI. If you change the ItemsSource to a new collection  in the middle of an
        /// edit on the  WPF DataGrid control, then the edit is cancelled, which we don't want
        /// as we would never be able to complete the edit.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal EditableImmutableListBridge<T> UpdateSource(ImmutableList<T> source)
        {
            // Note: We are inside a lock here so we don't have to worry about race conditions
            // Lock is entered further up the call stack

            var editableBridge = _guiCallerThread != Thread.CurrentThread ?
                new EditableImmutableListBridge<T>(source, _destination) :
                this;

            // Reset this to null here, in case the gui thread is waiting on a lock and we
            // aren't currently inside the gui thread we need to make sure that when the gui
            // thread gets to the line above it doesn't mutate the new state.
            _guiCallerThread = null;
            return editableBridge;
        }

        /// <summary>
        /// If used properly all incoming writes should only be coming from the GUI, and so the
        /// underlying collection needs to be syncronized immediately.
        /// </summary>
        private void Sync(Action action)
        {
            _guiCallerThread = Thread.CurrentThread;
            action();
            _source = _destination.ImmutableList;
            _guiCallerThread = null;
        }

        /// <summary>
        /// If used properly all incoming writes should only be coming from the GUI, and so the
        /// underlying collection needs to be syncronized immediately.
        /// </summary>
        private TResult Sync<TResult>(Func<TResult> action)
        {
            _guiCallerThread = Thread.CurrentThread;
            var returnValue = action();
            _source = _destination.ImmutableList;
            _guiCallerThread = null;
            return returnValue;
        }

        // **************************************************************************
        // IList<T> implementation
        // **************************************************************************
        #region IList<T> implementation

        public T this[int index]
        {
            get => _source[index];
            set => Sync(() => ((IList<T>)_destination)[index] = value);
        }

        public int Count => _source.Count;

        public bool IsReadOnly => false;

        public void Add(T item) => Sync(() => ((ICollection<T>)_destination).Add(item));

        public void Clear() => Sync(() => ((ICollection<T>)_destination).Clear());

        public bool Contains(T item) => ((ICollection<T>)_source).Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)_source).CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

        public int IndexOf(T item) => ((IList<T>)_source).IndexOf(item);

        public void Insert(int index, T item) => Sync(() => ((IList<T>)_destination).Insert(index, item));

        public bool Remove(T item) => Sync(() => ((ICollection<T>)_destination).Remove(item));

        public void RemoveAt(int index) => Sync(() => ((IList<T>)_destination).RemoveAt(index));

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();

        #endregion IList<T> implementation

        // **************************************************************************
        // IList implementation
        // **************************************************************************
        #region IList implementation

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        int ICollection.Count => _source.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => null;

        object IList.this[int index]
        {
            get => ((IList)_source)[index];
            set => Sync(() => ((IList)_destination)[index] = value);
        }

        int IList.Add(object value) => Sync(() => ((IList)_destination).Add(value));

        void IList.Clear() => Sync(() => ((IList)_destination).Clear());

        bool IList.Contains(object value) => ((IList)_source).Contains(value);

        int IList.IndexOf(object value) => ((IList)_source).IndexOf(value);

        void IList.Insert(int index, object value) => Sync(() => ((IList)_destination).Insert(index, value));

        void IList.Remove(object value) => Sync(() => ((IList)_destination).Remove(value));

        void IList.RemoveAt(int index) => Sync(() => ((IList)_destination).RemoveAt(index));

        void ICollection.CopyTo(Array array, int index) => ((IList)_source).CopyTo(array, index);

        #endregion IList implementation
    }
}