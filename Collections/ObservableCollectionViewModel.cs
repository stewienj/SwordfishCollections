// Authored by: John Stewien
// Year: 2011
// Company: Swordfish Computing
// License: 
// The Code Project Open License http://www.codeproject.com/info/cpol10.aspx
// Originally published at:
// http://www.codeproject.com/Articles/208361/Concurrent-Observable-Collection-Dictionary-and-So
// Last Revised: September 2012

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Swordfish.NET.Collections {

  /// <summary>
  /// This is the view model for the ConcurrentObservableCollectionBase that can be bound to a view.
  /// This is exposed by ConcurrentObservableCollectionBase when it is used from the Dispatcher thread.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObservableCollectionViewModel<T> : ObservableCollection<T>, IObserver<NotifyCollectionChangedEventArgs>, IDisposable {

    #region Private Fields

    /// <summary>
    /// Token that comes back when subscribing to the IObservable<> BaseCollection
    /// </summary>
    private IDisposable _unsubscribeToken = null;
    /// <summary>
    /// Token for removing the subscription action from the queue
    /// </summary>
    private IDisposable _subscriptionActionToken;

    #endregion Private Fields

    // ************************************************************************
    // Public Methods
    // ************************************************************************
    #region Public Methods

    /// <summary>
    /// Constructor. Queues subscribing to the IObservable<> passed in.
    /// </summary>
    /// <param name="observable"></param>
    public ObservableCollectionViewModel(IObservable<NotifyCollectionChangedEventArgs> observable) {

      // We create a subscribe action, which has a reference to this object.
      // If the DispatcherQueueProcessor isn't started (because the Dispatcher hasn't been
      // created), then the subscriber action will sit in the queue forever, hence will
      // never be garbage collection, hence this view model will never be garbage
      // collected. To get around this the dispatcher subscription queue stores a weak
      // reference. As such the Subscribe Action needs to be referenced in this class
      // otherwise it will be garbage collected once we leave the scope of this constructor.
      // the return token holds a reference to the Subscribe Action
      
      _subscriptionActionToken = DispatcherQueueProcessor.Instance.QueueSubscribe(() => {
        _unsubscribeToken = observable.Subscribe(this);
      });
    }

    /// <summary>
    /// Finalizer, disposes of the object
    /// </summary>
    ~ObservableCollectionViewModel() {
      Dispose(false);
    }

    /// <summary>
    ///  IObserver<> implementation
    /// </summary>
    public void OnCompleted() {
      // Nothing to do here
    }

    /// <summary>
    ///  IObserver<> implementation
    /// </summary>
    public void OnError(Exception error) {
      //TODO: handle it? log it?
    }

    /// <summary>
    ///  IObserver<> implementation
    /// </summary>
    public void OnNext(NotifyCollectionChangedEventArgs value) {
      DispatcherQueueProcessor.Instance.Add(() => {
        ProcessCommand(value);
      });
    }

    /// <summary>
    /// Disposes of the current object
    /// </summary>
    public void Dispose() {
      Dispose(true);
    }

    #endregion Public Methods

    // ************************************************************************
    // Private Methods
    // ************************************************************************
    #region Private Methods

    /// <summary>
    /// Processes a NotifyCollectionChangedEventArgs event argument
    /// </summary>
    /// <param name="command"></param>
    private void ProcessCommand(NotifyCollectionChangedEventArgs command) {
      switch(command.Action) {
        case NotifyCollectionChangedAction.Add: {
            int startIndex = command.NewStartingIndex;
            if(startIndex > -1) {
              foreach(var item in command.NewItems) {
                InsertItem(startIndex, (T)item);
                ++startIndex;
              }
            } else {
              foreach(var item in command.NewItems) {
                Add((T)item);
              }
            }
          }
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Remove: {
            int startIndex = command.OldStartingIndex;
            foreach(var item in command.OldItems) {
              RemoveAt(startIndex);
            }
          }
          break;
        case NotifyCollectionChangedAction.Replace: {
            int startIndex = command.OldStartingIndex;
            foreach(var item in command.NewItems) {
              this[startIndex] = (T)item;
              ++startIndex;
            }
          }
          break;
        case NotifyCollectionChangedAction.Reset:
          break;
      }
    }

    /// <summary>
    /// Disposes of this object, and supresses the finalizer
    /// </summary>
    /// <param name="isDisposing"></param>
    private void Dispose(bool isDisposing) {
      if(isDisposing) {
        GC.SuppressFinalize(this);
      }
      if(_subscriptionActionToken != null) {
        _subscriptionActionToken.Dispose();
      }
      if(_unsubscribeToken != null) {
        _unsubscribeToken.Dispose();
      }
    }

    #endregion Private Methods

  }
}
