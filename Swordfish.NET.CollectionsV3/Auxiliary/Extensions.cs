using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Swordfish.NET.Collections.Auxiliary
{
    /// <summary>
    ///     Contains general purpose extention methods.
    /// </summary>
    public static class Extensions
    {
        public static void Initialize<T>(this T[] list, T value)
        {
            for (int i = 0; i < list.Length; ++i)
            {
                list[i] = value;
            }
        }

        public static void Initialize<T>(this IList<T> list, T value)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                list[i] = value;
            }
        }

        public static bool ContainsKeysAll<T1, T2>(this Dictionary<T1, T2> dictionary, params T1[] keys)
        {
            foreach (var key in keys)
            {
                if (!dictionary.ContainsKey(key))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ContainsKeysAny<T1, T2>(this Dictionary<T1, T2> dictionary, params T1[] keys)
        {
            foreach (var key in keys)
            {
                if (dictionary.ContainsKey(key))
                {
                    return true;
                }
            }
            return false;
        }

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

        public static IEnumerable<IEnumerable<T>> BatchWithOverlap<T>(this IEnumerable<T> source, int batchSize, int overlapSize)
        {
            if (batchSize <= 0)
            {
                yield break;
            }
            if (overlapSize >= batchSize)
            {
                throw new ArgumentException("Overlap needs to be less than batch size", nameof(overlapSize));
            }
            List<T> batch = new List<T>();
            foreach (var item in source)
            {
                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch = batch.Skip(batchSize - overlapSize).ToList();
                }
                batch.Add(item);
            }
            yield return batch;
            yield break;
        }

        /// <summary>
        ///     Returns true if all items in the list are unique using
        ///     <see cref="EqualityComparer{T}.Default">EqualityComparer&lt;T&gt;.Default</see>.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <param name="source"/> is null.</exception>
        public static bool AllUnique<T>(this IList<T> source) where T : IEquatable<T>
        {
            Utils.RequireNotNull(source, "source");

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            return source.TrueForAllItemsToEachOtherItem((a, b) => !comparer.Equals(a, b));
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns true if <paramref name="compare"/> returns
        ///     true for every pair of items in <paramref name="source"/>.
        /// </summary>
        public static bool TrueForAllItemsToEachOtherItem<T>(this IList<T> source, Func<T, T, bool> compare)
        {
            Utils.RequireNotNull(source, "source");
            Utils.RequireNotNull(compare, "compare");

            for (int i = 0; i < source.Count; i++)
            {
                for (int j = i + 1; j < source.Count; j++)
                {
                    if (!compare(source[i], source[j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns true if <paramref name="compare"/> returns true of every
        ///     adjacent pair of items in the <paramref name="source"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     If there are n items in the collection, n-1 comparisons are done. 
        /// </para>
        /// <para>
        ///     Every valid [i] and [i+1] pair are passed into <paramref name="compare"/>.
        /// </para>
        /// <para>
        ///     If <paramref name="source"/> has 0 or 1 items, true is returned. 
        /// </para>
        /// </remarks>
        public static bool TrueForAllAdjacentPairs<T>(this IList<T> source, Func<T, T, bool> compare)
        {
            Utils.RequireNotNull(source, "source");
            Utils.RequireNotNull(compare, "compare");

            for (int i = 0; i < (source.Count - 1); i++)
            {
                if (!compare(source[i], source[i + 1]))
                {
                    return false;
                }
            }

            return true;
        }


        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns true if <paramref name="source"/> has no items in it; otherwise, false.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     If an <see cref="ICollection{TSource}"/> is provided,
        ///     <see cref="ICollection{TSource}.Count"/> is used.
        /// </para>
        /// <para>
        ///     Yes, this does basically the same thing as the
        ///     <see cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource})"/>
        ///     extention. The differences: 'IsEmpty' is easier to remember and it leverages 
        ///     <see cref="ICollection{TSource}.Count">ICollection.Count</see> if it exists.
        /// </para>    
        /// </remarks>
        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
        {
            Utils.RequireNotNull(source, "source");

            if (source is ICollection<TSource>)
            {
                return ((ICollection<TSource>)source).Count == 0;
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    return !enumerator.MoveNext();
                }
            }
        }

        //--------------------------------------------------------------------------------------------


        /// <summary>
        ///     Returns the index of the first item in <paramref name="source"/>
        ///     for which <paramref name="predicate"/> returns true. If none, -1.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        /// <param name="predicate">The function to evaluate on each element.</param>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Utils.RequireNotNull(source, "source");
            Utils.RequireNotNull(predicate, "predicate");

            int index = 0;
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns a new <see cref="ReadOnlyCollection{TSource}"/> using the
        ///     contents of <paramref name="source"/>.
        /// </summary>
        /// <remarks>
        ///     The contents of <paramref name="source"/> are copied to
        ///     an array to ensure the contents of the returned value
        ///     don't mutate.
        /// </remarks>
        public static ReadOnlyCollection<TSource> ToReadOnlyCollection<TSource>(this IEnumerable<TSource> source)
        {
            Utils.RequireNotNull(source, "source");
            return new ReadOnlyCollection<TSource>(source.ToArray());
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Performs the specified <paramref name="action"/>  
        ///     on each element of the specified <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to which is applied the specified <paramref name="action"/>.</param>
        /// <param name="action">The action applied to each element in <paramref name="source"/>.</param>
        public static IEnumerable<TSource> ForEach<TSource>(this IEnumerable<TSource> source, ForEachAction<TSource> action)
        {
            Utils.RequireNotNull(source, "source");
            Utils.RequireNotNull(action, "action");

            foreach (TSource item in source)
            {
                action(item);
            }

            // return the source so we can chain to other LINQ operators
            return source;
        }
        public delegate void ForEachAction<TSource>(TSource a);

        public static IEnumerable<TSource> ForEachNotNull<TSource>(this IEnumerable<TSource> source, ForEachAction<TSource> action)
        {
            Utils.RequireNotNull(source, "source");
            Utils.RequireNotNull(action, "action");

            foreach (TSource item in source)
            {
                if (item != null)
                {
                    action(item);
                }
            }

            // return the source so we can chain to other LINQ operators
            return source;
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     If <paramref name="source"/> is null, return an empty <see cref="IEnumerable{TSource}"/>;
        ///     otherwise, return <paramref name="source"/>.
        /// </summary>
        public static IEnumerable<TSource> EmptyIfNull<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? Enumerable.Empty<TSource>();
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Recursively projects each nested element to an <see cref="IEnumerable{TSource}"/>
        ///     and flattens the resulting sequences into one sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="recursiveSelector">A transform to apply to each element.</param>
        /// <returns>
        ///     An <see cref="IEnumerable{TSource}"/> whose elements are the
        ///     result of recursively invoking the recursive transform function
        ///     on each element and nested element of the input sequence.
        /// </returns>
        public static IEnumerable<TSource> SelectRecursive<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>> recursiveSelector)
        {
            Utils.RequireNotNull(source, "source");
            Utils.RequireNotNull(recursiveSelector, "recursiveSelector");

            Stack<IEnumerator<TSource>> stack = new Stack<IEnumerator<TSource>>();
            stack.Push(source.GetEnumerator());

            try
            {
                while (stack.Count > 0)
                {
                    if (stack.Peek().MoveNext())
                    {
                        TSource current = stack.Peek().Current;

                        yield return current;

                        stack.Push(recursiveSelector(current).GetEnumerator());
                    }
                    else
                    {
                        stack.Pop().Dispose();
                    }
                }
            }
            finally
            {
                while (stack.Count > 0)
                {
                    stack.Pop().Dispose();
                }
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T next)
        {
            return enumerable.Concat(ToIEnumerable(next));
        }

        public static IEnumerable<T> ToIEnumerable<T>(T next)
        {
            yield return next;
        }


        /// <summary>
        /// Performs action after iterating the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <example>
        /// int lineCount = 0;
        /// IEnumerable<string> lines =
        ///  File.ReadLines("filename.txt")
        ///  .BeginWith(() => lineCount = 0)
        ///  .EndWith(() => Console.WriteLine(lineCount))
        ///  .Select(x => { ++lineCount; return x; });
        /// </example>
        public static IEnumerable<T> EndWith<T>(this IEnumerable<T> enumerable, Action action)
        {
            // Copied from .NET Framework reference source Concat definition, the modified
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (action == null) throw new ArgumentNullException("action");
            foreach (var item in enumerable) yield return item;
            action();
        }

        /// <summary>
        /// Performs action before iterating the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        /// <example>
        /// int lineCount = 0;
        /// IEnumerable<string> lines =
        ///  File.ReadLines("filename.txt")
        ///  .BeginWith(() => lineCount = 0)
        ///  .EndWith(() => Console.WriteLine(lineCount))
        ///  .Select(x => { ++lineCount; return x; });
        /// </example>
        /// <returns></returns>
        public static IEnumerable<T> BeginWith<T>(this IEnumerable<T> enumerable, Action action)
        {
            // Copied from .NET Framework reference source Concat definition, the modified
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (action == null) throw new ArgumentNullException("action");
            action();
            foreach (var item in enumerable) yield return item;
        }

        public static void InitializeArray<T>(this Array array, Func<T> factory)
        {
            int[] recursiveIndicies = new int[0];
            Func<int[], T> objectFromIndicies = indices => factory();
            InitializeArray(array, objectFromIndicies, recursiveIndicies);
        }

        public static void InitializeArray<T>(this Array array, Func<int[], T> objectFromIndicies)
        {
            int[] recursiveIndicies = new int[0];
            InitializeArray(array, objectFromIndicies, recursiveIndicies);
        }

        private static void InitializeArray<T>(Array array, Func<int[], T> objectFromIndicies, int[] recursiveIndicies)
        {
            if (recursiveIndicies.Length < array.Rank)
            {
                int lowerBound = array.GetLowerBound(recursiveIndicies.Length);
                int upperBound = array.GetUpperBound(recursiveIndicies.Length);
                int dimensionLength = upperBound - lowerBound + 1;
                Enumerable.Range(lowerBound, dimensionLength).ForEach(x =>
                {
                    InitializeArray(array, objectFromIndicies, recursiveIndicies.Concat(x).ToArray());
                });
            }
            else
            {
                array.SetValue(objectFromIndicies(recursiveIndicies), recursiveIndicies);
            }
        }

        public static TValue GetValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            TValue returnValue;
            return dictionary.TryGetValue(key, out returnValue) ? returnValue : null;
        }

        public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            return dictionary.GetValueOrCreate(key, () => new TValue());
        }


        public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            TValue returnValue;
            if (!dictionary.TryGetValue(key, out returnValue))
            {
                returnValue = valueFactory();
            }
            return returnValue;
        }

        /// <summary>
        /// Match any number that is not preceded by a decimal point or a decimal point with numbers after
        /// </summary>
        private static Lazy<Regex> _numberFinder = new Lazy<Regex>(() => new Regex(@"(?<!\.\d*)\d+", RegexOptions.Compiled));
        /// <summary>
        /// Order items like this:
        /// abc2cc
        /// abc4cc
        /// abc20cc
        /// abc40cc
        /// abc200cc
        /// abc400cc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="maxDigits"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector, int maxDigits = 7)
        {
            return source.OrderBy(i => AlphaNumericReplacer(selector(i), maxDigits), StringComparer.Ordinal);
        }

        public static OrderedParallelQuery<T> OrderByAlphaNumeric<T>(this ParallelQuery<T> source, Func<T, string> selector, int maxDigits = 7)
        {
            return source.OrderBy(i => AlphaNumericReplacer(selector(i), maxDigits), StringComparer.Ordinal);
        }

        /// <summary>
        /// All this code below is just for handling negative numbers, else the code above could've just looked like this:
        /// return source.OrderBy(i => _numberFinder.Value.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')));
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxDigits"></param>
        /// <returns></returns>
        private static string AlphaNumericReplacer(string input, int maxDigits)
        {
            const char char9 = '9';
            const char char0 = '0';
            int lastStartPos = 0;
            StringBuilder sb = new StringBuilder();
            var matches = _numberFinder.Value.Matches(input);
            foreach (Match match in matches)
            {
                // Copy the text required up to the match, including any negative signs
                if (match.Index > lastStartPos)
                {
                    sb.Append(input.Substring(lastStartPos, match.Index - lastStartPos));
                }
                lastStartPos = match.Index + match.Length;

                // Pad the match out with zeros on the left
                var matchToCopy = match.Value.PadLeft(maxDigits, '0');

                // Test if negative by looking for the minus sign
                // If negative then subtract all the digits from 9, and pad out the numbers after the decimal point out to max digits as well
                bool isNegative = (match.Index > 0 && input[match.Index - 1] == '-');
                if (isNegative)
                {
                    char[] chars = matchToCopy.ToArray();
                    for (int i = 0; i < chars.Length; i++)
                    {
                        chars[i] = (char)(char9 - chars[i] + char0);
                    }
                    sb.Append(chars);

                    // Check if we have a decimal point in this negative number, and if so then subtract the digits from 9 and pad out to max digits
                    int fractionDigitsAdded = 0;
                    if (lastStartPos < input.Length && input[lastStartPos] == '.')
                    {
                        sb.Append('.');
                        lastStartPos++;
                        while (lastStartPos < input.Length && input[lastStartPos] >= char0 && input[lastStartPos] <= char9)
                        {
                            sb.Append((char)(char9 - input[lastStartPos] + char0));
                            lastStartPos++;
                            fractionDigitsAdded++;
                        }
                        if (fractionDigitsAdded < maxDigits)
                        {
                            sb.Append('9', maxDigits - fractionDigitsAdded);
                        }
                    }
                }
                else
                {
                    sb.Append(matchToCopy);
                }
            }
            if (lastStartPos < input.Length)
            {
                sb.Append(input.Substring(lastStartPos));
            }
            return sb.ToString();
        }


        /// <summary>
        /// Ensures the IEnumerable is not null
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable Ensure(this IEnumerable source)
        {
            return source ?? Enumerable.Empty<object>();
        }

        /// <summary>
        /// Ensures the IEnumerable is not null
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> Ensure<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
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

        public static IEnumerable<T> GetLatestConsumingEnumerable<T>(this BlockingCollection<T> collection)
        {
            foreach (T pos in collection.GetConsumingEnumerable())
            {
                T currentPos = pos;
                T nextPos;
                while (collection.TryTake(out nextPos))
                {
                    currentPos = nextPos;
                }
                yield return currentPos;
            }
        }


        /// <summary>
        /// Jon Skeet http://stackoverflow.com/questions/3683105/calculate-difference-from-previous-item-with-linq/3683217 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> SelectWithPrevious<TSource, TResult>(
          this IEnumerable<TSource> source,
          Func<TSource, TSource, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return projection(previous, iterator.Current);
                    previous = iterator.Current;
                }
            }
        }

        /// <summary>
        /// JCS my variant of the above Jon Skeet code
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="firstItem"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> SelectWithPreviousResult<TSource, TResult>(
          this IEnumerable<TSource> source,
          Func<TSource, TResult> firstItem,
          Func<TResult, TSource, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                TResult previousResult = firstItem(previous);
                yield return previousResult;
                while (iterator.MoveNext())
                {
                    previousResult = projection(previousResult, iterator.Current);
                    yield return previousResult;
                }
            }
        }
    }
}