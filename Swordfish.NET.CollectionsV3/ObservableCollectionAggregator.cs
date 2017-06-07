using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections {

  /// <summary>
  /// Aggregates multiple observable collections into 1.
  /// Not multithreadable, can only listen to updates on 1 thread.
  /// Can use with multiple ObservableCollectionView models as the source collection
  /// where the ObservableCollectionView is part of a ConcurrentObservableCollection
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObservableCollectionAggregator<T> : ObservableCollection<T> {

    private enum UpdateSourceType {
      NotUpdating,
      FromSource,
      FromDest
    }

    private UpdateSourceType _updateSource = UpdateSourceType.NotUpdating;
    private List<ObservableCollection<T>> _sourceCollections;
    private Dictionary<T, List<ObservableCollection<T>>> _itemToSourceCollectionOwners;

    public ObservableCollectionAggregator(params ObservableCollection<T>[] sourceCollections) {
      _sourceCollections = new List<ObservableCollection<T>>(sourceCollections);
      _itemToSourceCollectionOwners = new Dictionary<T, List<ObservableCollection<T>>>();
      foreach (var sourceCollection in _sourceCollections) {
        sourceCollection.CollectionChanged += sourceCollection_CollectionChanged;
      }
      this.CollectionChanged += Aggregated_CollectionChanged;
    }

    void Aggregated_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      if (_updateSource == UpdateSourceType.NotUpdating) {
        try {
          _updateSource = UpdateSourceType.FromDest;
          switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
              throw new ArgumentException("Can't add an item without an owner source");
            case NotifyCollectionChangedAction.Move:
              break;
            case NotifyCollectionChangedAction.Replace:
              throw new ArgumentException("Can't add an item without an owner source");
            case NotifyCollectionChangedAction.Remove:
              foreach (T key in e.OldItems) {
                List<ObservableCollection<T>> sourceCollectionOwners;
                if (_itemToSourceCollectionOwners.TryGetValue(key, out sourceCollectionOwners)) {
                  foreach (var collection in sourceCollectionOwners) {
                    while (collection.Remove(key)) ;
                  }
                  _itemToSourceCollectionOwners.Remove(key);
                }
              }
              break;
            case NotifyCollectionChangedAction.Reset:
              if (this.Count > 0) {
                throw new ArgumentException("Can't add an item without an owner source");
              }
              foreach (var collection in _sourceCollections) {
                collection.Clear();
              }
              break;
          }
        } finally {
          _updateSource = UpdateSourceType.NotUpdating;
        }
      }
    }

    private void AddRange(IEnumerable items, ObservableCollection<T> source) {
      foreach (T item in items) {
        List<ObservableCollection<T>> sourceCollectionOwners;
        if (_itemToSourceCollectionOwners.TryGetValue(item, out sourceCollectionOwners)) {
          if (!sourceCollectionOwners.Contains(source)) {
            sourceCollectionOwners.Add(source);
          }
        } else {
          sourceCollectionOwners = new List<ObservableCollection<T>>();
          _itemToSourceCollectionOwners[item] = sourceCollectionOwners;
          sourceCollectionOwners.Add(source);
          this.Add(item);
        }
      }
    }

    private void RemoveRange(IEnumerable items, ObservableCollection<T> source) {
      foreach (T item in items) {
        List<ObservableCollection<T>> sourceCollectionOwners;
        if (_itemToSourceCollectionOwners.TryGetValue(item, out sourceCollectionOwners)) {
          sourceCollectionOwners.Remove(source);
          if (sourceCollectionOwners.Count < 1) {
            this.Remove(item);
            _itemToSourceCollectionOwners.Remove(item);
          }
        }
      }
    }

    void sourceCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      if (_updateSource == UpdateSourceType.NotUpdating) {
        try {
          _updateSource = UpdateSourceType.FromSource;

          switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
              this.AddRange(e.NewItems, (ObservableCollection<T>)sender);
              break;
            case NotifyCollectionChangedAction.Move:
              break;
            case NotifyCollectionChangedAction.Remove:
              this.RemoveRange(e.OldItems, (ObservableCollection<T>)sender);
              break;
            case NotifyCollectionChangedAction.Replace:
              this.RemoveRange(e.OldItems, (ObservableCollection<T>)sender);
              this.AddRange(e.NewItems, (ObservableCollection<T>)sender);
              break;
            case NotifyCollectionChangedAction.Reset:
              this.Clear();
              _itemToSourceCollectionOwners.Clear();
              foreach (var collection in _sourceCollections) {
                AddRange(collection, collection);
              }
              break;
          }
        } finally {
          _updateSource = UpdateSourceType.NotUpdating;
        }
      }
    }
  }
}

