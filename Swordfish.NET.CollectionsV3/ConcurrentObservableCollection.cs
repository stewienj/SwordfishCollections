using Swordfish.NET.General;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Immutable;
using System.Threading;
using System.Collections.Concurrent;
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
  public class ConcurrentObservableCollection<T> :
    // Use IList<T> as the internal collection type parameter, not ImmutableList
    // otherwise everything that uses this needs to reference the corresponding assembly
    ConcurrentObservableBase<T, IList<T>>, 
    IList<T>,
    IList,
    ISerializable
  {

    public ConcurrentObservableCollection() : this(true)
    {
    }

    private ImmutableList<T> ImmutableList
    {
      get
      {
        return (ImmutableList<T>)_internalCollection;
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
    }

    public T RemoveLast()
    {
      return DoReadWriteNotify(
        () => new { Index = ImmutableList.Count - 1, Item = ImmutableList.LastOrDefault()},
        (indexAndItem) => indexAndItem.Index < 0 ? ImmutableList : ImmutableList.RemoveAt(indexAndItem.Index),
        (indexAndItem) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, indexAndItem.Item)
      ).Item;
    }

    /// <summary>
    /// Adds a range of items to the end of the collection. Quicker than adding them individually,
    /// but the view doesn't update until the last item has been added.
    /// </summary>
    public void AddRange(IList<T> items)
    {
      DoReadWriteNotify(
        () => ImmutableList.Count,
        (index) => ImmutableList.AddRange(items),
        (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, index)
      );
    }

    /// <summary>
    /// Inserts a range of items at the position specified. *Much quicker* than adding them
    /// individually, but the view doesn't update until the last item has been inserted.
    /// </summary>
    public void InsertRange(int index, IList<T> items)
    {
      DoWriteNotify(
        () => ImmutableList.InsertRange(index, items),
        () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, index)
      );
    }

    /// <summary>
    /// Rmoves a range of items by index and count
    /// </summary>
    public void RemoveRange(int index, int count)
    {
      DoReadWriteNotify(
        ()=> ImmutableList.GetRange(index, count),
        (items)=> ImmutableList.RemoveRange(index, count),
        (items)=> new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items, index)
      );
    }

    public void RemoveRange(IList<T> items)
    {
      DoWriteNotify(
        () => ImmutableList.RemoveRange(items),
        () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items)
      );
    } 

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
      return ImmutableList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ImmutableList.GetEnumerator();
    }

    #endregion IEnumerable<T> Implementation


    // ************************************************************************
    // IList<T> Implementation
    // ************************************************************************
    #region IList<T> Implementation

    public int IndexOf(T item)
    {
      return ImmutableList.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      DoWriteNotify(
        () => ImmutableList.Insert(index, item),
        () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item , index)
      );
    }

    public void RemoveAt(int index)
    {
      DoReadWriteNotify(
        ()=> ImmutableList[index],
        (item)=> ImmutableList.RemoveAt(index),
        (item)=> new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item , index)
      );
    }

    public T this[int index]
    {
      get
      {
        return ImmutableList[index];
      }
      set
      {
        DoReadWriteNotify(
          () => ImmutableList[index],
          (item) => ImmutableList.SetItem(index, value),
          (item) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item, index)
        );
      }
    }

    #endregion IList<T> Implementation

    // ************************************************************************
    // ICollection<T> Implementation
    // ************************************************************************
    #region ICollection<T> Implementation

    public void Add(T item)
    {
      DoReadWriteNotify(
        ()=> ImmutableList.Count,
        (index)=> ImmutableList.Add(item),
        (index)=> new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
      );
    }

    public void Clear()
    {
      DoReadWriteNotify(
        () => ImmutableList.ToArray(),
        (items) => ImmutableList.Clear(),
        (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0)
      );
    }

    public bool Contains(T item)
    {
      return ImmutableList.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      ImmutableList.CopyTo(array, arrayIndex);
    }

    public override int Count
    {
      get
      {
        return ImmutableList.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public override IList<T> CollectionView
    {
      get
      {
        return ImmutableList;
      }
    }

    public bool Remove(T item)
    {
      return DoReadWriteNotify(
        () => ImmutableList.IndexOf(item),
        (index) => index < 0 ? ImmutableList : ImmutableList.RemoveAt(index),
        (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index)
      ) >= 0;
    }

    #endregion ICollection<T> Implementation

    // ************************************************************************
    // ICollection Implementation
    // ************************************************************************
    #region ICollection Implementation

    void ICollection.CopyTo(Array array, int index)
    {
      ((ICollection)ImmutableList).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return ((ICollection)ImmutableList).IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return ((ICollection)ImmutableList).SyncRoot;
      }
    }

    #endregion ICollection Implementation

    // ************************************************************************
    // IList Implementation
    // ************************************************************************
    #region IList Implementation

    int IList.Add(object value)
    {
      return DoReadWriteNotify(
        ()=> ImmutableList.Count,
        (index)=> ImmutableList.Add((T)value),
        (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index)
      );
    }

    bool IList.Contains(object value)
    {
        return ((IList)ImmutableList).Contains(value);
    }

    int IList.IndexOf(object value)
    {
        return ((IList)ImmutableList).IndexOf(value);
    }

    void IList.Insert(int index, object value)
    {
      this.Insert(index, (T)value);
    }

    bool IList.IsFixedSize
    {
      get
      {
          return ((IList)ImmutableList).IsFixedSize;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
          return ((IList)ImmutableList).IsReadOnly;
      }
    }

    void IList.Remove(object value)
    {
      this.Remove((T)value);
    }

    void IList.RemoveAt(int index)
    {
      this.RemoveAt(index);
    }

    object IList.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        this[index] = (T)value;
      }
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
    #endregion
  }
}
