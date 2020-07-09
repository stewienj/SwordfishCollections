using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Swordfish.NET.Collections
{
    /// <summary>
    /// Generates ObservableCollectionDeduplicator objects. Takes a base-source collection and a
    /// converter function to build up a map to convert back and forth between another source
    /// collection and a deduplicated version of it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionDeduplicatorFactory<K, T>
    {

        private enum UpdateSourceType
        {
            NotUpdating,
            FromSource,
            FromDest
        }

        UpdateSourceType _updateSource = UpdateSourceType.NotUpdating;

        /// <summary>
        /// The source collection to be aggregated (optionally by string)
        /// </summary>
        ObservableCollection<K> _source;
        /// <summary>
        /// The aggregated collection
        /// </summary>
        ObservableCollection<T> _aggregated;
        /// <summary>
        /// The bridge that maps string to items in the source collection
        /// </summary>
        Dictionary<T, List<K>> _bridge;
        /// <summary>
        /// Converts from the source collection type to the output collection type
        /// </summary>
        Func<K, T> _convert;

        public ObservableCollectionDeduplicatorFactory(ObservableCollection<K> source, Func<K, T> convert)
        {
            _convert = convert;
            _bridge = new Dictionary<T, List<K>>();
            _aggregated = new ObservableCollection<T>();
            _source = source;
            _source.CollectionChanged += Source_CollectionChanged;
            _aggregated.CollectionChanged += Aggregated_CollectionChanged;
        }

        public ObservableCollectionDeduplicator<K, T> CreateBidirectionalDeduplicator(ObservableCollection<K> source, ObservableCollection<T> dest)
        {
            Func<K, T> convertTo = (k) => _convert(k);
            Func<T, IEnumerable<K>> convertFrom = (t) => _bridge.ContainsKey(t) ? _bridge[t] : Enumerable.Empty<K>();
            return new ObservableCollectionDeduplicator<K, T>(source, dest, this);
        }

        public ObservableCollectionDeduplicator<K, T> CreateBidirectionalDeduplicator()
        {
            return CreateBidirectionalDeduplicator(new ObservableCollection<K>(), new ObservableCollection<T>());
        }

        public ObservableCollectionDeduplicator<K, T> CreateBidirectionalDeduplicator(ObservableCollection<K> source)
        {
            return CreateBidirectionalDeduplicator(source, new ObservableCollection<T>());
        }

        public ObservableCollectionDeduplicator<K, T> CreateBidirectionalDeduplicator(ObservableCollection<T> dest)
        {
            return CreateBidirectionalDeduplicator(new ObservableCollection<K>(), dest);
        }

        public Func<K, T> ConvertTo
        {
            get
            {
                return _convert;
            }
            set
            {
                _convert = value;
            }
        }

        public Func<T, IEnumerable<K>> ConvertFrom
        {
            get
            {
                return (t) => _bridge.ContainsKey(t) ? _bridge[t] : Enumerable.Empty<K>();
            }
        }

        public ObservableCollection<K> Source
        {
            get
            {
                return _source;
            }
        }

        /// <summary>
        /// Returns an aggregated collection that shouldn't be modified externally as this is the master aggregated source.
        /// </summary>
        public ObservableCollection<T> AggregatedSource
        {
            get
            {
                return _aggregated;
            }
        }

        private void AddItem(K item)
        {
            T key = _convert(item);
            if (_bridge.ContainsKey(key))
            {
                _bridge[key].Add(item);
            }
            else
            {
                _bridge.Add(key, new List<K>());
                _bridge[key].Add(item);
                _aggregated.Add(key);
            }
        }

        private void RemoveItem(K item)
        {
            T key = _convert(item);
            if (_bridge.ContainsKey(key))
            {
                _bridge[key].Remove(item);
                if (_bridge[key].Count < 1)
                {
                    _bridge.Remove(key);
                    _aggregated.Remove(key);
                }
            }
            else
            {
                _aggregated.Remove(key);
            }
        }

        private void AddRange(IEnumerable items)
        {
            foreach (K item in items)
            {
                AddItem(item);
            }
        }

        private void RemoveRange(IEnumerable items)
        {
            foreach (K item in items)
            {
                this.RemoveItem(item);
            }
        }

        void Aggregated_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updateSource == UpdateSourceType.NotUpdating)
            {
                try
                {
                    _updateSource = UpdateSourceType.FromDest;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            throw new InvalidOperationException("Can't handle adding items aggregated value with no original value");
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            throw new InvalidOperationException("Can't handle adding items aggregated value with no original value");
                        case NotifyCollectionChangedAction.Remove:
                            foreach (T key in e.OldItems)
                            {
                                if (_bridge.ContainsKey(key))
                                {
                                    foreach (K item in _bridge[key])
                                    {
                                        _source.Remove(item);
                                    }
                                    _aggregated.Remove(key);
                                    _bridge.Remove(key);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _source.Clear();
                            if (_aggregated.Count > 0)
                            {
                                throw new InvalidOperationException("Can't handle adding items aggregated value with no original value");
                            }
                            break;
                    }
                }
                finally
                {
                    _updateSource = UpdateSourceType.NotUpdating;
                }
            }
        }

        private void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_updateSource == UpdateSourceType.NotUpdating)
            {
                try
                {
                    _updateSource = UpdateSourceType.FromSource;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            this.AddRange(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            this.RemoveRange(e.OldItems);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            this.RemoveRange(e.OldItems);
                            this.AddRange(e.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _aggregated.Clear();
                            _bridge.Clear();
                            AddRange(_source);
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

