using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Swordfish.NET.Collections
{

  /// <summary>
  /// ConcurrentBag that implements the INotifyCollectionChanged
  /// Uses a ConcurrentBag underneath, but triggers the CollectionChanged
  /// event when items are added or successfully removed.
  /// 
  /// Implemented INotifyPropertyChanged to allow it to be a replacement for 
  /// ObservableCollection. 
  /// 
  /// The collection/property change events will be sent on the UI thread. 
  /// 
  /// The intended use is to replace ObservableCollection where the collection is updated on a nonUI thread
  /// but the collection is bound to UI display elements. It will probably stuff up if you try to use the 
  /// collection change events to trigger updates on itself
  /// 
  /// Note that : SyncRoot and IsSynchronized properties do not trigger a PropertyChanged event
  /// but that shouldn't matter.
  /// </summary>

  public class ObservableConcurrentBag<T> :
    IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {

    public ObservableConcurrentBag()
    {
      concurrentBag = new ConcurrentBag<T>();
    }

    public ObservableConcurrentBag(IEnumerable<T> collection)
    {
      concurrentBag = new ConcurrentBag<T>(collection);
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public int Count { get { return concurrentBag.Count; } }
    public bool IsEmpty { get { return concurrentBag.IsEmpty; } }

    public void Add(T item)
    {
      concurrentBag.Add(item);
      //inform listeners that the item was added, 
      InvokeChanges(NotifyCollectionChangedAction.Add, item);
    }

    public void CopyTo(T[] array, int index)
    {
      concurrentBag.CopyTo(array, index);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return concurrentBag.GetEnumerator();
    }
    public T[] ToArray()
    {
      return concurrentBag.ToArray();
    }

    public bool TryPeek(out T item)
    {
      return concurrentBag.TryPeek(out item);
    }

    public bool TryTake(out T item)
    {
      var success = concurrentBag.TryTake(out item);
      //only inform listeners if items was successfully removed 
      if (success)
        InvokeChanges(NotifyCollectionChangedAction.Remove, item);
      return success;
    }

    public void Clear()
    {
      //to match ObservableCollection
      var list = new List<T>(concurrentBag.ToArray());
      concurrentBag = new ConcurrentBag<T>();
      InvokeChanges(NotifyCollectionChangedAction.Reset, null);
    }



    //Explicitly implementing these parts of the interfaces because that matches 
    //the implementation of ConcurentBag


    bool IProducerConsumerCollection<T>.TryAdd(T item)
    {
      var temp = ((IProducerConsumerCollection<T>)concurrentBag).TryAdd(item);
      InvokeChanges(NotifyCollectionChangedAction.Add, item);
      return temp;
    }

    void ICollection.CopyTo(Array array, int index)
    {
      ((ICollection)concurrentBag).CopyTo(array, index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)concurrentBag).GetEnumerator();
    }

    int ICollection.Count { get { return concurrentBag.Count; } }

    object ICollection.SyncRoot { get { return ((ICollection)concurrentBag).SyncRoot; } }

    bool ICollection.IsSynchronized { get { return ((ICollection)concurrentBag).IsSynchronized; } }



    //the item doing all the actual work
    private ConcurrentBag<T> concurrentBag;

    private void InvokeChanges(NotifyCollectionChangedAction action, object item)
    {

      //invoke the collection/property changes on the UI thread
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (item == null)
          CollectionChanged?.Invoke(this,new NotifyCollectionChangedEventArgs(action));
        else
          CollectionChanged?.Invoke(this,new NotifyCollectionChangedEventArgs(action, item));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
      });

    }
  }
}
