using Swordfish.NET.Collections.Auxiliary;
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
    public class ConcurrentObservableHashSet<T> :
    ConcurrentObservableBase<T, ImmutableHashSet<T>>,
    ICollection<T>,
    ISet<T>,
    ICollection,
    ISerializable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConcurrentObservableHashSet() : this(isMultithreaded: true, comparer: null, controlledAction: null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">Custom item comparer</param>
        public ConcurrentObservableHashSet(IEqualityComparer<T> comparer) : this(isMultithreaded: true, comparer, controlledAction: null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isMultithreaded">Whether collection supports updates from multiple threads</param>
        /// <param name="comparer">Custom item comparer, null indicates to use default</param>
        /// <param name="controlledAction">Override the ThottledAction with an implementation of IControlledAction</param>
        public ConcurrentObservableHashSet(bool isMultithreaded = true, IEqualityComparer<T> comparer = null, IControlledAction controlledAction = null) : base(isMultithreaded, ImmutableHashSet<T>.Empty.WithComparer(comparer), controlledAction)
        {
        }

        public bool Add(T value)
        {
            bool wasAdded = false;
            DoWriteNotify(
              () =>
              {
                  var newCollection = _internalCollection.Add(value);
                  wasAdded = newCollection != _internalCollection;
                  return newCollection;
              },
              () => wasAdded ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value) : null
            );
            return wasAdded;
        }

        /// <summary>
        /// Adds a range of items to the end of the collection. Quicker than adding them individually,
        /// but the view doesn't update until the last item has been added.
        /// </summary>
        public void AddRange(IEnumerable<T> values)
        {
            DoWriteNotify(
              () => _internalCollection.AddRange(values),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, values as IList ?? values.ToList())
            );
        }

        public bool Remove(T value)
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


        public void RemoveRange(IEnumerable<T> values)
        {
            DoWriteNotify(
              () => _internalCollection.RemoveRange(values),
              () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, values as IList ?? values.ToList())
            );
        }

        /// <summary>
        /// This is the view of the colleciton that you should be binding to with your ListView/GridView control.
        /// </summary>
        public override IList<T> CollectionView => _internalCollection.ToList();

        public override int Count => _internalCollection.Count;

        public bool IsReadOnly => false;

        public override string ToString()
        {
            return $"{{Items : {Count}}}";
        }

        // ************************************************************************
        // IEnumerable<T> Implementation
        // ************************************************************************
        #region IEnumerable<T> Implementation

        public IEnumerator<T> GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }

        #endregion IEnumerable<T> Implementation

        public void Clear()
        {
            DoReadWriteNotify(
              // Get the list of keys and values from the internal list
              () => _internalCollection.ToList(),
              // remove the keys from the dictionary, remove the range from the list
              (items) => ImmutableHashSet<T>.Empty,
              // Notify which items were removed
              (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0)
            );
        }

        public bool Contains(T item)
        {
            return _internalCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)_internalCollection).CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int arrayIndex) => ((ICollection)_internalCollection).CopyTo(array, arrayIndex);

        object ICollection.SyncRoot => ((ICollection)_internalCollection).SyncRoot;

        bool ICollection.IsSynchronized => ((ICollection)_internalCollection).IsSynchronized;

        void ICollection<T>.Add(T item) => Add(item);

        void ISet<T>.UnionWith(IEnumerable<T> other) => ((ISet<T>)_internalCollection).UnionWith(other);

        void ISet<T>.IntersectWith(IEnumerable<T> other) => ((ISet<T>)_internalCollection).IntersectWith(other);

        void ISet<T>.ExceptWith(IEnumerable<T> other) => ((ISet<T>)_internalCollection).ExceptWith(other);

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => ((ISet<T>)_internalCollection).SymmetricExceptWith(other);

        bool ISet<T>.IsSubsetOf(IEnumerable<T> other) => _internalCollection.IsSubsetOf(other);

        bool ISet<T>.IsSupersetOf(IEnumerable<T> other) => _internalCollection.IsSupersetOf(other);

        bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other) => _internalCollection.IsProperSupersetOf(other);

        bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other) => _internalCollection.IsProperSubsetOf(other);

        bool ISet<T>.Overlaps(IEnumerable<T> other) => _internalCollection.Overlaps(other);

        bool ISet<T>.SetEquals(IEnumerable<T> other) => _internalCollection.SetEquals(other);

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

        protected ConcurrentObservableHashSet(SerializationInfo information, StreamingContext context) : base(information, context)
        {
            var children = (T[])information.GetValue("children", typeof(T[]));
            _internalCollection = ImmutableHashSet<T>.Empty.AddRange(children);
        }
        #endregion
    }
}
