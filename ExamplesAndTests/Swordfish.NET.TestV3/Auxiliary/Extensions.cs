using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.TestV3.Auxiliary
{
    internal static class Extensions
    {
        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (batchSize == 0)
            {
                yield break;
            }
            List<T> retVal = new List<T>();
            int count = 0;
            foreach (var item in source)
            {
                if (count >= batchSize)
                {
                    yield return retVal;
                    count = 0;
                    retVal = new List<T>();
                }
                retVal.Add(item);
                count++;
            }
            yield return retVal;
        }

        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> source)
        {
            var list = collection as List<T>;
            if (list != null)
            {
                list.AddRange(source);
            }
            else
            {
                source.ForEach(x => collection.Add(x));
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> collection, IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            source.ForEach(x => collection.Add(x.Key, x.Value));
        }

        public static IEnumerable<TSource> ForEach<TSource>(this IEnumerable<TSource> source, ForEachAction<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }

            // return the source so we can chain to other LINQ operators
            return source;
        }
        public delegate void ForEachAction<TSource>(TSource a);
    }
}
