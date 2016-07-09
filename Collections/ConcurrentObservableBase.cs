// Authored by: John Stewien
// Year: 2011
// Company: Swordfish Computing
// License: 
// The Code Project Open License http://www.codeproject.com/info/cpol10.aspx
// Originally published at:
// http://www.codeproject.com/Articles/208361/Concurrent-Observable-Collection-Dictionary-and-So
// Last Revised: September 2012

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using System.Collections.ObjectModel;

namespace Swordfish.NET.Collections {

  /// <summary>
  /// This class provides the base for concurrent collections that 
  /// can be bound to user interface elements
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <remarks>
  /// TODO: Use ReaderWriterLockSlim instead of ReaderWriterLock
  /// </remarks>
  public abstract class ConcurrentObservableBase<T> :
    IObservable<NotifyCollectionChangedEventArgs>,
    INotifyCollectionChanged,
    IEnumerable<T>,
    IDisposable {

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// The lock that controls read/write access to the base collection
    /// </summary>
    private ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
    /// <summary>
    /// The underlying base enumerable that is used to store the items,
    /// used for creating an immutable collection from which an enumerator
    /// can be obtained.
    /// </summary>
    private ObservableCollection<T> _baseCollection;

    // Could do with a more performant enumerable implementation
    // but this is what I have so far. I create a snapshot of the collection
    // and use the enumerable from that. When the collection is updated
    // I set a flag indicating that a new snapshot is required.

    /// <summary>
    /// Flag indicating that a write has occured, so anything that depends on
    /// taking a snapshot of the collection needs to be updated.
    /// </summary>
    private bool _newSnapshotRequired = false;
    /// <summary>
    /// The enumerable lock to prevent threading conflicts on allocating
    /// the enumerable of the fixed collection
    /// </summary>
    private ReaderWriterLockSlim _snapshotLock = new ReaderWriterLockSlim();
    /// <summary>
    /// The collection used for generating an enumerable that iterates
    /// over a snapshot of the base collection
    /// </summary>
    private ImmutableCollectionBase<T> _baseSnapshot;

    /// <summary>
    /// A list of observers
    /// </summary>
    private Dictionary<int, IObserver<NotifyCollectionChangedEventArgs>> _subscribers;
    /// <summary>
    /// The key for new observers, incremented with each new observer
    /// </summary>
    private int _subscriberKey;
    /// <summary>
    /// Flag indicating this collection is disposed
    /// </summary>
    private bool _isDisposed;
    /// <summary>
    /// The view model that is used to allow this collection to be bound to the UI.
    /// Relevant methods determine if they are being called on the UI thread, and if
    /// so then the view model is used.
    /// </summary>
    private ObservableCollectionViewModel<T> _viewModel;

    #endregion Private Fields

    // ************************************************************************
    // Constructors
    // ************************************************************************
    #region Constructors

    /// <summary>
    /// Default Constructor
    /// </summary>
    protected ConcurrentObservableBase() : this(new T[]{}) {
    }

    /// <summary>
    /// Constructor that takes an eumerable
    /// </summary>
    protected ConcurrentObservableBase(IEnumerable<T> enumerable) {
      _subscribers = new Dictionary<int, IObserver<NotifyCollectionChangedEventArgs>>();
      
      _baseCollection = new ObservableCollection<T>(enumerable);
      _baseSnapshot = new ImmutableCollection<T>(enumerable);

      // subscribers must be initialized befor calling this as it may
      // subscribe immediately
      _viewModel = new ObservableCollectionViewModel<T>(this);

      // Handle when the base collection changes. Event will be passed through
      // the IObservable.OnNext method.
      _baseCollection.CollectionChanged += HandleBaseCollectionChanged;

      // Bubble up the notify collection changed event from the view model
      _viewModel.CollectionChanged += (sender, e) => {
        if(CollectionChanged != null) {
          CollectionChanged(sender, e);
        }
      };
    }

    ~ConcurrentObservableBase() {
      Dispose(false);
    }

    #endregion Constructors

    // ************************************************************************
    // Protected Methods
    // ************************************************************************
    #region Protected Methods

    /// <summary>
    /// Removes all items from the ICollection<T>.
    /// </summary>
    protected void DoBaseClear(Action action) {
      // Don't use BaseCollection.Clear(), it causes problems because it
      // sends a reset event, and then the collection needs to be read out through
      // an enumerator. Use RemoveAt instead until the collection is empty.
      // Using remove from end after testing with this speed test:
      //
      //  // speed test for using RemoveAt(int index);
      //  using System;
      //  using System.Collections.Generic;
      //  using System.Collections.ObjectModel;
      //  using System.Diagnostics;
      //
      //  namespace ConsoleApplication1 {
      //    class Program {
      //      static void Main(string[] args) {
      //        var coll = new Collection<int>();
      //        for (int ix = 0; ix < 100000; ++ix) coll.Add(ix);
      //        var sw = Stopwatch.StartNew();
      //        while (coll.Count > 0) coll.RemoveAt(0);
      //        sw.Stop();
      //        Console.WriteLine("Removed from start {0}ms",sw.ElapsedMilliseconds);
      //        for (int ix = 0; ix < 100000; ++ix) coll.Add(ix);
      //        sw = Stopwatch.StartNew();
      //        while (coll.Count > 0) coll.RemoveAt(coll.Count - 1);
      //        Console.WriteLine("Removed from end {0}ms",sw.ElapsedMilliseconds);
      //        Console.ReadLine();
      //      }
      //    }
      //  }
      //
      //  Output: 
      //  Removed from start 4494ms
      //  Removed from end 3ms

      // Need a special case of DoBaseWrite for a set changes to make sure that nothing else does a change
      // while we are in the middle of doing a collection of changes.

      _readWriteLock.TryEnterUpgradeableReadLock(Timeout.Infinite);
      try {
        _readWriteLock.TryEnterWriteLock(Timeout.Infinite);
        action();
        while(WriteCollection.Count > 0) {
          _newSnapshotRequired = true;
          WriteCollection.RemoveAt(WriteCollection.Count - 1);
        }
      } finally {
        if(_readWriteLock.IsWriteLockHeld) {
          _readWriteLock.ExitWriteLock();
        }
        _readWriteLock.ExitUpgradeableReadLock();
      }
    }

    /// <summary>
    /// Handles when the base collection changes. Pipes the event through IObservable.OnNext
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void HandleBaseCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

      // As this is a concurrent collection we don't want a change event to result in the listener
      // later coming back to enumerate over the whole collection again, possible before the listener
      // gets other changed events, but after the collection has been added to.

      bool actionTypeIsOk = e.Action != NotifyCollectionChangedAction.Reset;
      System.Diagnostics.Debug.Assert(actionTypeIsOk, "Reset called on concurrent observable collection. This shouldn't happen");

      OnNext(e);
    }

    /// <summary>
    /// Updates the snapshot that is used to generate an Enumerator
    /// </summary>
    /// <param name="forceUpdate"></param>
    private void UpdateSnapshot() {
      if(_newSnapshotRequired) {
        _snapshotLock.TryEnterWriteLock(Timeout.Infinite);
        if(_newSnapshotRequired) {
          _baseSnapshot = new ImmutableCollection<T>(_baseCollection);
          _newSnapshotRequired = false;
        }
        _snapshotLock.ExitWriteLock();
      }
    }

    /// <summary>
    /// Handles read access from the base collection
    /// </summary>
    /// <param name="readFunc"></param>
    protected void DoBaseRead(Action readFunc) {
      DoBaseRead<object>(() => {
        readFunc();
        return null;
      });
    }

    /// <summary>
    /// Handles read access from the base collection when a return value is required
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="readFunc"></param>
    /// <returns></returns>
    protected TResult DoBaseRead<TResult>(Func<TResult> readFunc) {
      if(IsDispatcherThread) {
        return readFunc();
      }

      _readWriteLock.TryEnterReadLock(Timeout.Infinite);
      try {
        return readFunc();
      } finally {
        _readWriteLock.ExitReadLock();
      }
    }

    /// <summary>
    /// Calls the read function passed in, and if it returns true,
    /// then calls the next read function, else calls the write
    /// function.
    /// </summary>
    protected TResult DoBaseReadWrite<TResult>(Func<bool> readFuncTest, Func<TResult> readFunc, Func<TResult> writeFunc) {
      _readWriteLock.TryEnterUpgradeableReadLock(Timeout.Infinite);
      try {
        if(readFuncTest()) {
          return readFunc();
        } else {
          _readWriteLock.TryEnterWriteLock(Timeout.Infinite);
          try {
            _newSnapshotRequired = true;
            TResult returnValue = writeFunc();
            return returnValue;
          } finally {
            if(_readWriteLock.IsWriteLockHeld) {
              _readWriteLock.ExitWriteLock();
            }
          }
        }
      } finally {
        _readWriteLock.ExitUpgradeableReadLock();
      }
    }

    /// <summary>
    /// Calls the read function passed in, and if it returns true,
    /// then calls the next read function, else unlocks the collection,
    /// calls the pre-write function, then chains to DoBaseReadWrite
    /// calls the write
    /// function.
    /// </summary>
    protected TResult DoBaseReadWrite<TResult>(Func<bool> readFuncTest, Func<TResult> readFunc, Action preWriteFunc, Func<TResult> writeFunc) {
      _readWriteLock.TryEnterReadLock(Timeout.Infinite);
      try {
        if(readFuncTest()) {
          return readFunc();
        }
      } finally {
        _readWriteLock.ExitReadLock();
      }
      preWriteFunc();
      return DoBaseReadWrite(readFuncTest, readFunc, writeFunc);
    }

    /// <summary>
    /// Handles write access to the base collection
    /// </summary>
    /// <param name="writeFunc"></param>
    protected void DoBaseWrite(Action writeFunc) {
      DoBaseWrite<object>(() => {
        writeFunc();
        return null;
      });
    }

    /// <summary>
    /// Handles write access to the base collection when a return value is required
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="writeFunc"></param>
    /// <returns></returns>
    protected TResult DoBaseWrite<TResult>(Func<TResult> writeFunc) {
      _readWriteLock.TryEnterUpgradeableReadLock(Timeout.Infinite);
      try {
        _readWriteLock.TryEnterWriteLock(Timeout.Infinite);
        _newSnapshotRequired = true;
        return writeFunc();
      } finally {
        if(_readWriteLock.IsWriteLockHeld) {
          _readWriteLock.ExitWriteLock();
        }
        _readWriteLock.ExitUpgradeableReadLock();
      }
    }

    #endregion Protected Methods

    // ************************************************************************
    // Properties
    // ************************************************************************
    #region Properties

    /// <summary>
    /// Gets an immutable snapshot of the collection
    /// </summary>
    public ImmutableCollectionBase<T> Snapshot {
      get {
        return DoBaseRead(() => {
          UpdateSnapshot();
          return _baseSnapshot;
        });
      }
    }

    /// <summary>
    /// Gets the base collection that holds the values
    /// </summary>
    protected ObservableCollection<T> WriteCollection {
      get {
        return _baseCollection;
      }
    }

    protected ObservableCollection<T> ReadCollection {
      get {
        if(IsDispatcherThread) {
          return ViewModel;
        } else {
          return WriteCollection;
        }
      }
    }

    /// <summary>
    /// Access this directly if getting the error "An ItemsControl is inconsistent with its items source".
    /// </summary>
    protected ObservableCollectionViewModel<T> ViewModel {
      get {
        return _viewModel;
      }
    }

    /// <summary>
    /// Gets if the calling thread is the same as the dispatcher thread
    /// </summary>
    protected static bool IsDispatcherThread {
      get {
        return DispatcherQueueProcessor.Instance.IsDispatcherThread;
      }
    }

    #endregion Properties

    // ************************************************************************
    // IObservable Implementation
    // ************************************************************************
    #region IObservable Implementation

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if(disposing) {
        GC.SuppressFinalize(this);
      }
      OnCompleted();
      _isDisposed = true;
    }

    protected void OnNext(NotifyCollectionChangedEventArgs value) {
      if(_isDisposed) {
        throw new ObjectDisposedException("Observable<T>");
      }

      foreach(IObserver<NotifyCollectionChangedEventArgs> observer in _subscribers.Select(kv => kv.Value)) {
        observer.OnNext(value);
      }
    }

    protected void OnError(Exception exception) {
      if(_isDisposed) {
        throw new ObjectDisposedException("Observable<T>");
      }

      if(exception == null) {
        throw new ArgumentNullException("exception");
      }

      foreach(IObserver<NotifyCollectionChangedEventArgs> observer in _subscribers.Select(kv => kv.Value)) {
        observer.OnError(exception);
      }
    }

    protected void OnCompleted() {
      if(_isDisposed) {
        throw new ObjectDisposedException("Observable<T>");
      }

      foreach(IObserver<NotifyCollectionChangedEventArgs> observer in _subscribers.Select(kv => kv.Value)) {
        observer.OnCompleted();
      }
    }

    public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer) {
      if(observer == null) {
        throw new ArgumentNullException("observer");
      }
      return DoBaseWrite(() => {
        int key = _subscriberKey++;
        _subscribers.Add(key, observer);
        UpdateSnapshot();
        foreach(var item in _baseSnapshot) {
          observer.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        return new DoDispose(() => {
          DoBaseWrite(() => {
            _subscribers.Remove(key);
          });
        });
      });
    }

    /// <summary>
    /// Used as the return IDisposable from Subscribe()
    /// </summary>
    private class DoDispose : IDisposable {
      private Action _doDispose;
      public DoDispose(Action doDispose) {
        _doDispose = doDispose;
      }

      public void Dispose() {
        _doDispose();
      }
    }

    #endregion IObservable Implementation

    // ************************************************************************
    // INotifyCollectionChanged Implementation
    // ************************************************************************
    #region INotifyCollectionChanged Implementation

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion INotifyCollectionChanged Implementation

    // ************************************************************************
    // IEnumerable<T> Implementation
    // ************************************************************************
    #region IEnumerable<T> Implementation
    /// <summary>
    /// Gets the enumerator for a snapshot of the collection
    /// </summary>
    /// <remarks>
    /// Note that the Enumerator should really only be used on the Dispatcher thread,
    /// if not then should enumerate over the Snapshot instead.
    /// 
    /// </remarks>
    public IEnumerator<T> GetEnumerator() {
      if(IsDispatcherThread) {
        return _viewModel.GetEnumerator();
      } else {
        return Snapshot.GetEnumerator();
      }
    }

    /// <summary>
    /// Gets the enumerator for a snapshot of the collection
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    #endregion IEnumerable<T> Implementation

  }
}
