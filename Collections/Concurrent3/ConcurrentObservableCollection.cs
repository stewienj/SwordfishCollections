using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Immutable;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Concurrent;
using Swordfish.NET.General;

namespace Swordfish.NET.Collections.Concurrent3
{
  /// <summary>
  /// A collection that can be updated from multiple threads, and can be bound to an items control in the user interface.
  /// Has the advantage over ObservableCollection in that it doesn't have to be updated from the Dispatcher thread.
  /// When using this in your view model you should bind to the CollectionView property in your view model. If you
  /// bind directly this this class it will throw an exception.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ConcurrentObservableCollection<T> :
    NotifyPropertyChanged,
    INotifyCollectionChanged,
    IEnumerable<T>,
    IList<T>,
    ICollection<T>,
    IList,
    ICollection
  {
    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// The lock that controls read/write access to the base collection when it's been initialized as thread safe.
    /// Allows updating the collection from multiple threads.
    /// </summary>
    private ReaderWriterLockSlim _lock;
    /// <summary>
    /// The internal collection that holds all the items and provides enumerators, etc
    /// </summary>
    private ImmutableList<T> _internalCollection = ImmutableList<T>.Empty;
    /// <summary>
    /// A throttle for the "CollectionView" PropertyChanged event. Experimented with using this / not using this, and
    /// found there was a 25% performance gain from using this.
    /// </summary>
    private ThrottledAction _viewChanged;

    #endregion Private Fields

    public ConcurrentObservableCollection() : this(true)
    {

    }

    /// <summary>
    /// Constructructor. Takes an optional isMultithreaded argument where when true allows you to update the collection
    /// from multiple threads. In testing there didn't seem to be any performance hit from turning this on, so I made
    /// it the default.
    /// </summary>
    /// <param name="isThreadSafe"></param>
    public ConcurrentObservableCollection(bool isMultithreaded)
    {
      _viewChanged = new ThrottledAction(() => OnPropertyChanged(nameof(CollectionView), nameof(Count)), TimeSpan.FromMilliseconds(20));
      _lock = isMultithreaded ? new ReaderWriterLockSlim() : null;
    }

    /// <summary>
    /// Adds a range of items to the end of the collection. Quicker than adding them individually,
    /// but the view doesn't update until the last item has been added.
    /// </summary>
    public void AddRange(IList<T> items)
    {
      DoReadWriteNotify(
        () => _internalCollection.Count,
        (index) => _internalCollection.AddRange(items),
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
        () => _internalCollection.InsertRange(index, items),
        () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, index)
      );
    }

    /// <summary>
    /// Rmoves a range of items by index and count
    /// </summary>
    public void RemoveRange(int index, int count)
    {
      DoReadWriteNotify(
        ()=> _internalCollection.GetRange(index, count),
        (items)=> _internalCollection.RemoveRange(index, count),
        (items)=> new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items, index)
      );
    }

    public void RemoveRange(IList<T> items)
    {
      DoWriteNotify(
        () => _internalCollection.RemoveRange(items),
        () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items)
      );
    } 

    /// <summary>
    /// Freezes updates if the collection was created as multithreaded. To resume updates dispose of the returned IDisposable.
    /// </summary>
    /// <returns></returns>
    public IDisposable FreezeUpdates()
    {
      _lock?.EnterReadLock();
      return new AnonDisposable(() =>
      {
        _lock?.ExitReadLock();
      });
    }

    /// <summary>
    /// This is the view of the colleciton that you should be binding to with your ListView/GridView control
    /// </summary>
    public IList<T> CollectionView
    {
      get
      {
        return _internalCollection;
      }
    }

    protected void DoWriteNotify(Func<ImmutableList<T>> write, Func<NotifyCollectionChangedEventArgs> change)
    {
      DoReadWriteNotify(() => 0, n => write(), n => change());
    }

    protected TRead DoReadWriteNotify<TRead>(Func<TRead> read, Func<TRead, ImmutableList<T>> write, Func<TRead, NotifyCollectionChangedEventArgs> change)
    {
      _lock?.EnterUpgradeableReadLock();
      TRead readValue = read();
      _lock?.EnterWriteLock();
      _internalCollection = write(readValue);
      var changeValue = change(readValue);
      OnCollectionChanged(changeValue);
      _lock?.ExitWriteLock();
      _lock?.ExitUpgradeableReadLock();
      _viewChanged.InvokeAction();
      return readValue;
    }

    public override string ToString()
    {
      return $"{{Items : {Count}}}";
    }

    // ************************************************************************
    // INotifyCollectionChanged Implementation
    // ************************************************************************
    #region INotifyCollectionChanged Implementation

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs changes)
    {
      try
      {
        _collectionChanged?.Invoke(this, changes);
      }
      catch (Exception)
      {

      }
    }

    private event NotifyCollectionChangedEventHandler _collectionChanged;
    public event NotifyCollectionChangedEventHandler CollectionChanged
    {
      add
      {
        if (value.Target is System.Windows.Data.CollectionView)
        {
          throw new ApplicationException($"Collection type={typeof(T).Name}, don't bind directly to {nameof(ConcurrentObservableCollection<T>)}, instead bind to {nameof(ConcurrentObservableCollection<T>)}.CollectionView");
          // Try binding to CollectionView instead
          // Note that if you do comment out the above you'll get an inconsistent
          // collection exception if you update the collection while the gui is updating.
        }
        _collectionChanged += value;
      }
      remove
      {
        _collectionChanged -= value;
      }
    }


    #endregion INotifyCollectionChanged Implementation

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


    // ************************************************************************
    // IList<T> Implementation
    // ************************************************************************
    #region IList<T> Implementation

    public int IndexOf(T item)
    {
      return _internalCollection.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      DoWriteNotify(
        () => _internalCollection.Insert(index, item),
        () => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item , index)
      );
    }

    public void RemoveAt(int index)
    {
      DoReadWriteNotify(
        ()=> _internalCollection[index],
        (item)=> _internalCollection.RemoveAt(index),
        (item)=> new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item , index)
      );
    }

    public T this[int index]
    {
      get
      {
        return _internalCollection[index];
      }
      set
      {
        DoReadWriteNotify(
          () => _internalCollection[index],
          (item) => _internalCollection.SetItem(index, value),
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
        ()=> _internalCollection.Count,
        (index)=> _internalCollection.Add(item),
        (index)=> new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
      );
    }

    public void Clear()
    {
      DoReadWriteNotify(
        () => _internalCollection.ToArray(),
        (items) => _internalCollection.Clear(),
        (items) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0)
      );
    }

    public bool Contains(T item)
    {
      return _internalCollection.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      _internalCollection.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get
      {
        return _internalCollection.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public bool Remove(T item)
    {
      return DoReadWriteNotify(
        () => _internalCollection.IndexOf(item),
        (index) => index < 0 ? _internalCollection : _internalCollection.RemoveAt(index),
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
      ((ICollection)_internalCollection).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return ((ICollection)_internalCollection).IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return ((ICollection)_internalCollection).SyncRoot;
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
        ()=> _internalCollection.Count,
        (index)=> _internalCollection.Add((T)value),
        (index) => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index)
      );
    }

    bool IList.Contains(object value)
    {
        return ((IList)_internalCollection).Contains(value);
    }

    int IList.IndexOf(object value)
    {
        return ((IList)_internalCollection).IndexOf(value);
    }

    void IList.Insert(int index, object value)
    {
      this.Insert(index, (T)value);
    }

    bool IList.IsFixedSize
    {
      get
      {
          return ((IList)_internalCollection).IsFixedSize;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
          return ((IList)_internalCollection).IsReadOnly;
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
  }
}
