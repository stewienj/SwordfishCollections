using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;

namespace Swordfish.NET.Collections
{
    public static class ConcurrentObservableExtensions
    {

        public static void MapChildListToAggregate<T, K>(this ConcurrentObservableCollection<T> source, ConcurrentObservableCollection<K> dest, Func<T, ConcurrentObservableCollection<K>> selectChildList)
        {
            NotifyCollectionChangedEventHandler collectionChangedHander = (s, e) =>
            {
                foreach (var item in e?.OldItems?.OfType<T>() ?? Enumerable.Empty<T>())
                {
                    selectChildList(item).UnmapChildToAggregate(dest);
                }
                foreach (var item in e?.NewItems?.OfType<T>() ?? Enumerable.Empty<T>())
                {
                    selectChildList(item).MapChildToAggregate(dest);
                }
            };

            using (var freezeToken = source.FreezeUpdates())
            {
                source.CollectionChanged += collectionChangedHander;
                foreach (var item in source)
                {
                    selectChildList(item).MapChildToAggregate(dest);
                }
            }
        }


        public static void MapChildToAggregate<T>(this ConcurrentObservableCollection<T> source, ConcurrentObservableCollection<T> dest)
        {
            source.ChildToAggregateMapUnmap(dest, MapUnmap.Map);
        }
        public static void UnmapChildToAggregate<T>(this ConcurrentObservableCollection<T> source, ConcurrentObservableCollection<T> dest)
        {
            source.ChildToAggregateMapUnmap(dest, MapUnmap.Unmap);
        }

        private enum MapUnmap
        {
            Map,
            Unmap
        }

        private static void ChildToAggregateMapUnmap<T>(this ConcurrentObservableCollection<T> source, ConcurrentObservableCollection<T> dest, MapUnmap mapUnmap)
        {
            NotifyCollectionChangedEventHandler collectionChangedHander = (s, e) =>
            {
                foreach (var item in e?.OldItems?.OfType<T>() ?? Enumerable.Empty<T>())
                {
                    dest.Remove(item);
                }
                foreach (var item in e?.NewItems?.OfType<T>() ?? Enumerable.Empty<T>())
                {
                    dest.Add(item);
                }
            };

            using (var freezeToken = source.FreezeUpdates())
            {
                if (mapUnmap == MapUnmap.Map)
                {
                    dest.AddRange(source);
                    source.CollectionChanged += collectionChangedHander;
                }
                else if (mapUnmap == MapUnmap.Unmap)
                {
                    dest.RemoveRange(source);
                    source.CollectionChanged -= collectionChangedHander;
                }
            }
        }

        internal static ImmutableHashSet<T> AddRange<T>(this ImmutableHashSet<T> dest, IEnumerable<T> source)
        {
            var returnValue = dest;
            foreach (var i in source)
            {
                returnValue = returnValue.Add(i);
            }
            return returnValue;
        }

        internal static ImmutableHashSet<T> RemoveRange<T>(this ImmutableHashSet<T> target, IEnumerable<T> source)
        {
            var returnValue = target;
            foreach (var i in source)
            {
                returnValue = returnValue.Remove(i);
            }
            return returnValue;
        }
    }
}
