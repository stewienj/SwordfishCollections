using Swordfish.NET.General;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections
{
  public interface IConcurrentObservableBase<T> : INotifyPropertyChanged, INotifyCollectionChanged
  {
    IList<T> CollectionView { get; }

    int Count { get; }
  }

  public abstract class ConcurrentObservableBase<T,TInternalCollection> :
    NotifyPropertyChanged,
    IConcurrentObservableBase<T>
    where TInternalCollection : class 
  {
    /// <summary>
    /// The lock that controls read/write access to the base collection when it's been initialized as thread safe.
    /// Allows updating the collection from multiple threads.
    /// </summary>
    private ReaderWriterLockSlim _lock;

    /// <summary>
    /// The internal collection that holds all the keys, but the keys then point to linklist node which holds the values.
    /// </summary>
    protected TInternalCollection _internalCollection;

    /// <summary>
    /// A throttle for the "CollectionView" PropertyChanged event. Experimented with using throttling / not using throttling, and
    /// found there was a 25% performance gain from using throttling.
    /// </summary>
    private ThrottledAction _viewChanged;

    protected ConcurrentObservableBase(bool isMultithreaded, TInternalCollection initialCollection)
    {
      _lock = isMultithreaded ? new ReaderWriterLockSlim() : null;
      _internalCollection = initialCollection;
      _viewChanged = new ThrottledAction(() => OnPropertyChanged(nameof(CollectionView), nameof(Count)), TimeSpan.FromMilliseconds(20));
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


    protected void DoWriteNotify(Func<TInternalCollection> write, Func<NotifyCollectionChangedEventArgs> change)
    {
      DoReadWriteNotify(() => 0, n => write(), n => change());
    }

    private TRead BodyReadWriteNotify<TRead>(TRead readValue, Func<TRead, TInternalCollection> write, Func<TRead, NotifyCollectionChangedEventArgs> change)
    {
      _lock?.EnterWriteLock();
      _internalCollection = write(readValue);
      var changeValue = change(readValue);
      if (changeValue != null)
      {
        OnCollectionChanged(changeValue);
      }
      _lock?.ExitWriteLock();
      _lock?.ExitUpgradeableReadLock();
      _viewChanged.InvokeAction();
      return readValue;
    }

    protected TRead DoReadWriteNotify<TRead>(Func<TRead> read, Func<TRead, TInternalCollection> write, Func<TRead, NotifyCollectionChangedEventArgs> change)
    {
      _lock?.EnterUpgradeableReadLock();
      TRead readValue = read();
      return BodyReadWriteNotify(readValue, write, change);
    }

    protected bool DoTestReadWriteNotify<TRead>(Func<bool> test, Func<TRead> read, Func<TRead, TInternalCollection> write, Func<TRead, NotifyCollectionChangedEventArgs> change)
    {
      _lock?.EnterUpgradeableReadLock();

      bool testResult = test();
      if (testResult)
      {
        TRead readValue = read();
        BodyReadWriteNotify(readValue, write, change);
      }
      else
      {
        _lock?.ExitUpgradeableReadLock();
      }
      return testResult;
    }


    protected bool DoTestReadWriteNotify<TRead>(
      Func<bool> test,
      Func<TRead> readTrue, Func<TRead, TInternalCollection> writeTrue, Func<TRead, NotifyCollectionChangedEventArgs> changeTrue,
      Func<TRead> readFalse, Func<TRead, TInternalCollection> writeFalse, Func<TRead, NotifyCollectionChangedEventArgs> changeFalse
      )
    {
      _lock?.EnterUpgradeableReadLock();

      bool testResult = test();

      var read = testResult ? readTrue : readFalse;
      var write = testResult ? writeTrue : writeFalse;
      var change = testResult ? changeTrue : changeFalse;

      TRead readValue = read();
      BodyReadWriteNotify(readValue, write, change);
      return testResult;
    }

    /// <summary>
    /// This is the view of the colleciton that you should be binding to with your ListView/GridView control.
    /// </summary>
    public abstract IList<T> CollectionView
    {
      get;
    }

    public abstract int Count
    {
      get;
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
  }
}
