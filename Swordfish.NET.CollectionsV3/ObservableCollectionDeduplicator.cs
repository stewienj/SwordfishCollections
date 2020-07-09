using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Swordfish.NET.Collections
{

    /// <summary>
    /// This class has 2 ObservableCollection objects. One (source) holds arbitrary objects, the second holds
    /// a copy of the first but where duplicates have been removed. This class also converts the source class to
    /// another type (e.g. string), and deduplicates on the resultant object.
    /// 
    /// The collections are bidrectional, so removing from one or adding to one updates the other, however a map is
    /// required to map the objects between each other. So when I add 1 object to the deduplicated class, it looks up
    /// in the map for a list of objects to add to the source class. This mapping is handled by a factory that
    /// generated this object - ObservableCollectionDeduplicatorFactory
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionDeduplicator<K, T>
    {
        private enum UpdateSourceType
        {
            NotUpdating,
            FromSource,
            FromDest
        }

        UpdateSourceType _updateSource = UpdateSourceType.NotUpdating;

        private Dictionary<T, int> _itemCount = new Dictionary<T, int>();
        private ObservableCollection<K> _source;
        private ObservableCollection<T> _deduplicated;
        private ObservableCollectionDeduplicatorFactory<K, T> _factory;

        public ObservableCollectionDeduplicator(ObservableCollection<K> source, ObservableCollection<T> dest, ObservableCollectionDeduplicatorFactory<K, T> factory)
        {
            _factory = factory;
            _source = source;
            _deduplicated = dest;
            AddRangeSourceToDest(_source);
            _source.CollectionChanged += Source_CollectionChanged;
            _deduplicated.CollectionChanged += DeduplicatedDest_CollectionChanged;
            _factory.Source.CollectionChanged += MasterSource_CollectionChanged;
        }

        private void MasterSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (K item in e.OldItems)
                    {
                        if (_source.Contains(item))
                        {
                            _source.Remove(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (K item in e.OldItems)
                    {
                        if (_source.Contains(item))
                        {
                            _source.Remove(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _source.Clear();
                    _deduplicated.Clear();
                    break;
            }
        }

        public ObservableCollection<K> Source
        {
            get
            {
                return _source;
            }
        }
        public ObservableCollection<T> Deduplicated
        {
            get
            {
                return _deduplicated;
            }
        }

        private void AddRangeDestToSource(IList items)
        {
            foreach (T itemT in items)
            {
                foreach (K itemK in _factory.ConvertFrom(itemT))
                {
                    _source.Add(itemK);
                }
            }
        }

        private void RemoveRangeDestToSource(IList items)
        {
            foreach (T itemT in items)
            {
                foreach (K itemK in _factory.ConvertFrom(itemT))
                {
                    _source.Remove(itemK);
                }
            }
        }

        private void AddRangeSourceToDest(IList items)
        {
            foreach (K itemK in items)
            {
                var itemT = _factory.ConvertTo(itemK);
                if (!_itemCount.ContainsKey(itemT))
                {
                    _deduplicated.Add(itemT);
                    _itemCount[itemT] = 1;
                }
                else
                {
                    _itemCount[itemT] += 1;
                }
            }
        }

        private void RemoveRangeSourceToDest(IList items)
        {
            foreach (K itemK in items)
            {
                var itemT = _factory.ConvertTo(itemK);
                if (_itemCount.ContainsKey(itemT))
                {
                    _itemCount[itemT] -= 1;
                    if (_itemCount[itemT] < 1)
                    {
                        _itemCount.Remove(itemT);
                        _deduplicated.Remove(itemT);
                    }
                }
                else
                {
                    _deduplicated.Remove(itemT);
                }
            }
        }

        private void DeduplicatedDest_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updateSource == UpdateSourceType.NotUpdating)
            {
                try
                {
                    _updateSource = UpdateSourceType.FromDest;
                    // Don't allow duplicates in list
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            AddRangeDestToSource(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            RemoveRangeDestToSource(e.OldItems);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            // Remove old items
                            RemoveRangeDestToSource(e.OldItems);
                            // Add new items
                            AddRangeDestToSource(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _source.Clear();
                            _itemCount.Clear();
                            AddRangeDestToSource(_deduplicated);
                            break;
                    }
                }
                finally
                {
                    _updateSource = UpdateSourceType.NotUpdating;
                }
            }
        }

        private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updateSource == UpdateSourceType.NotUpdating)
            {
                try
                {
                    _updateSource = UpdateSourceType.FromSource;
                    // Don't allow duplicates in list
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            AddRangeSourceToDest(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            RemoveRangeSourceToDest(e.OldItems);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            // Remove old items
                            RemoveRangeSourceToDest(e.OldItems);
                            // Add new items
                            AddRangeSourceToDest(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _deduplicated.Clear();
                            _itemCount.Clear();
                            AddRangeSourceToDest(_source);
                            break;
                    }
                }
                finally
                {
                    _updateSource = UpdateSourceType.NotUpdating;
                }
            }
        }
    }
}
