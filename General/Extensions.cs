using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Text;



namespace Swordfish.NET.General
{
  /// <summary>
  ///     Contains general purpose extention methods.
  /// </summary>
  public static class Extensions
  {
    public static string ReplaceCaseInsensitive(this string str, string oldValue, string newValue)
    {
      return Regex.Replace(str,
        Regex.Escape(oldValue),
        Regex.Replace(newValue, "\\$[0-0]+", @"$$$0"),
        RegexOptions.IgnoreCase);
    }

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

    /// <summary>
    /// Really fast integer exponent
    /// from http://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    /// </summary>
    /// <param name="x"></param>
    /// <param name="pow"></param>
    /// <returns></returns>
    public static long Pow(this int x, int pow)
    {
      long temp = x;
      pow = Math.Max(0, pow);
      // Throw an exception it this overflows
      checked
      {
        long ret = 1;
        while (pow != 0)
        {
          if ((pow & 1) == 1)
            ret *= temp;
          temp *= temp;
          pow >>= 1;
        }
        return ret;
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

      return source.TrueForAllPairs((a, b) => !comparer.Equals(a, b));
    }

    //--------------------------------------------------------------------------------------------

    /// <summary>
    ///     Returns true if <paramref name="compare"/> returns
    ///     true for every pair of items in <paramref name="source"/>.
    /// </summary>
    public static bool TrueForAllPairs<T>(this IList<T> source, Func<T, T, bool> compare)
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
    ///     Returns true if all of the items in <paramref name="source"/> are not 
    ///     null or empty.
    /// </summary>
    /// <exception cref="ArgumentNullException">if <param name="source"/> is null.</exception>
    public static bool AllNotNullOrEmpty(this IEnumerable<string> source)
    {
      Utils.RequireNotNull(source, "source");
      return source.All(item => !string.IsNullOrEmpty(item));
    }

    //--------------------------------------------------------------------------------------------

    /// <summary>
    ///     Returns true if all items in <paramref name="source"/> exist
    ///     in <paramref name="set"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">if <param name="source"/> or <param name="set"/> are null.</exception>
    public static bool AllExistIn<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> set)
    {
      Utils.RequireNotNull(source, "source");
      Utils.RequireNotNull(set, "set");

      return source.All(item => set.Contains(item));
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

    //--------------------------------------------------------------------------------------------

    /// <summary>
    /// Perform a deep Copy of the object.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T SerializableClone<T>(this T source)
    {
      if (!source.GetType().IsSerializable)
      {
        throw new ArgumentException("The type must be serializable.", "source");
      }

      // Don't serialize a null object, simply return the default for that object
      if (Object.ReferenceEquals(source, null))
      {
        return default(T);
      }

      IFormatter formatter = new BinaryFormatter();
      Stream stream = new MemoryStream();
      using (stream)
      {
        formatter.Serialize(stream, source);
        stream.Seek(0, SeekOrigin.Begin);
        return (T)formatter.Deserialize(stream);
      }
    }

    //--------------------------------------------------------------------------------------------

    private static Dictionary<Type, Dictionary<string, object>> enumTypeToConverters = new Dictionary<Type, Dictionary<string, object>>();

    /// <summary>
    /// Gets the description for the enumerated type
    /// </summary>
    /// <param name="currentEnum"></param>
    /// <returns></returns>
    public static string GetEnumDescription(this Enum currentEnum)
    {
      return GetEnumAttribute<DescriptionAttribute>(currentEnum)
        ?.Description 
        ?? currentEnum.ToString();
    }

    public static TAttr GetEnumAttribute<TAttr>(this Enum currentEnum) where TAttr : Attribute
    {
      FieldInfo fieldInfo = currentEnum.GetType().GetField(currentEnum.ToString());
      TAttr enumAttr = null;
      if (fieldInfo != null)
        enumAttr = (TAttr)Attribute.GetCustomAttribute(fieldInfo, typeof(TAttr));
      return enumAttr;
    }

    /// <summary>
    /// Converts a description back to an enumerated type. Slight pain
    /// that we can't constrain T to be type Enum in .NET 4.0, so use
    /// the dynamic keyword to get around that little issue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="description"></param>
    /// <returns></returns>
    public static Enum DescriptionToEnum<T>(this string description)
    {

      object retVal = DescriptionToEnum(description, typeof(T));
      if (retVal is T)
      {
        return retVal as Enum;
      }
      else
      {
        return default(T) as Enum;
      }
    }

    public static object DescriptionToEnum(this string description, Type t)
    {
      // Check that we've pre-created a converter, and if not then
      // create a new one.
      if (!enumTypeToConverters.ContainsKey(t))
      {
        var dictionary = new Dictionary<string, object>();
        enumTypeToConverters[t] = dictionary;
        foreach (dynamic g in Enum.GetValues(t))
        {
          dictionary[GetEnumDescription(g)] = g;
        }
      }

      var converter = enumTypeToConverters[t];

      object value;
      if (converter.TryGetValue(description, out value))
      {
        return value;
      }
      else
      {
        return null;
      }
    }

    //--------------------------------------------------------------------------------------------

    public static T XmlSerializeClone<T>(this T source)
    {
      // Don't serialize a null object, simply return the default for that object
      if (Object.ReferenceEquals(source, null))
      {
        return default(T);
      }

      XmlSerializer serializer = new XmlSerializer(typeof(T));
      Stream stream = new MemoryStream();
      using (stream)
      {
        serializer.Serialize(stream, source);
        stream.Seek(0, SeekOrigin.Begin);
        return (T)serializer.Deserialize(stream);
      }
    }

    /// <summary>
    /// Converts a double to Engineering Notation
    /// </summary>
    /// <example>
    /// class Program {
    ///   static void Main(string[] args) {
    ///     for (int i = -18; i &lt;= 28; ++i) {
    ///       double a = Math.Pow(10, i) * 1.234567890123;
    ///       Console.WriteLine("{0} -> {1}Hz",a,a.ToEngineeringNotation());
    ///     }
    ///     Console.ReadLine();
    ///   }
    /// }
    /// 
    /// Gives the following output
    /// 
    /// 1.234567890123E-18 -> 0.00123aHz
    /// 1.234567890123E-17 -> 0.0123aHz
    /// 1.234567890123E-16 -> 0.123aHz
    /// 1.234567890123E-15 -> 1.23fHz
    /// 1.234567890123E-14 -> 12.3fHz
    /// 1.234567890123E-13 -> 123fHz
    /// 1.234567890123E-12 -> 1.23pHz
    /// 1.234567890123E-11 -> 12.3pHz
    /// 1.234567890123E-10 -> 123pHz
    /// 1.234567890123E-09 -> 1.23nHz
    /// 1.234567890123E-08 -> 12.3nHz
    /// 1.234567890123E-07 -> 123nHz
    /// 1.234567890123E-06 -> 1.23µHz
    /// 1.234567890123E-05 -> 12.3µHz
    /// 0.0001234567890123 -> 123µHz
    /// 0.001234567890123 -> 1.23mHz
    /// 0.01234567890123 -> 12.3mHz
    /// 0.1234567890123 -> 123mHz
    /// 1.234567890123 -> 1.23Hz
    /// 12.34567890123 -> 12.3Hz
    /// 123.4567890123 -> 123Hz
    /// 1234.567890123 -> 1.23kHz
    /// 12345.67890123 -> 12.3kHz
    /// 123456.7890123 -> 123kHz
    /// 1234567.890123 -> 1.23MHz
    /// 12345678.90123 -> 12.3MHz
    /// 123456789.0123 -> 123MHz
    /// 1234567890.123 -> 1.23GHz
    /// 12345678901.23 -> 12.3GHz
    /// 123456789012.3 -> 123GHz
    /// 1234567890123 -> 1.23THz
    /// 12345678901230 -> 12.3THz
    /// 123456789012300 -> 123THz
    /// 1.234567890123E+15 -> 1.23PHz
    /// 1.234567890123E+16 -> 12.3PHz
    /// 1.234567890123E+17 -> 123PHz
    /// 1.234567890123E+18 -> 1.23EHz
    /// 1.234567890123E+19 -> 12.3EHz
    /// 1.234567890123E+20 -> 123EHz
    /// 1.234567890123E+21 -> 1.23ZHz
    /// 1.234567890123E+22 -> 12.3ZHz
    /// 1.234567890123E+23 -> 123ZHz
    /// 1.234567890123E+24 -> 1.23YHz
    /// 1.234567890123E+25 -> 12.3YHz
    /// 1.234567890123E+26 -> 123YHz
    /// 1.234567890123E+27 -> 1235YHz
    /// 1.234567890123E+28 -> 12346YHz
    /// </example>
    /// <param name="d">the double to convert</param>
    /// <param name="significantFigures">The number of significant figures</param>
    /// <returns>A string</returns>
    public static string ToEngineeringNotation(this double d, int significantFigures = 3)
    {

      // Here's a lambda funtion for formatting a number that ranges between 1 and 999
      Func<double, string> format = (x) =>
      {
        int decimalPlaces = significantFigures - ((int)Math.Floor(Math.Log10(Math.Abs(x))) + 1);
        if (decimalPlaces >= 0)
        {
          return x.ToString("F" + decimalPlaces.ToString());
        }
        // Need to start zeroing out figures that come to the left of the decimal place. Divide by powers of 10
        // and pad out with zeros
        var retVal = (x * Math.Pow(10, decimalPlaces)).ToString("F0").ToCharArray().Concat(Enumerable.Repeat('0', -decimalPlaces));
        return new String(retVal.ToArray());
      };

      // Convert the double to a number between 1 and 999
      // Format it with the above function
      // Add the appropriate suffix
      double exponent = Math.Log10(Math.Abs(d));
      if (Math.Abs(d) >= 1)
      {
        switch ((int)Math.Floor(exponent))
        {
          case 0:
          case 1:
          case 2:
            return format(d);
          case 3:
          case 4:
          case 5:
            return format(d / 1e3) + "k";
          case 6:
          case 7:
          case 8:
            return format(d / 1e6) + "M";
          case 9:
          case 10:
          case 11:
            return format(d / 1e9) + "G";
          case 12:
          case 13:
          case 14:
            return format(d / 1e12) + "T";
          case 15:
          case 16:
          case 17:
            return format(d / 1e15) + "P";
          case 18:
          case 19:
          case 20:
            return format(d / 1e18) + "E";
          case 21:
          case 22:
          case 23:
            return format(d / 1e21) + "Z";
          default:
            return format(d / 1e24) + "Y";
        }
      }
      else if (Math.Abs(d) > 0)
      {
        switch ((int)Math.Floor(exponent))
        {
          case -1:
          case -2:
          case -3:
            return format(d * 1e3) + "m";
          case -4:
          case -5:
          case -6:
            return format(d * 1e6) + "μ";
          case -7:
          case -8:
          case -9:
            return format(d * 1e9) + "n";
          case -10:
          case -11:
          case -12:
            return format(d * 1e12) + "p";
          case -13:
          case -14:
          case -15:
            return format(d * 1e15) + "f";
          case -16:
          case -17:
          case -18:
            return format(d * 1e18) + "a";
          case -19:
          case -20:
          case -21:
            return format(d * 1e21) + "z";
          default:
            return format(d * 1e24) + "y";
        }
      }
      else
      {
        return "0";
      }
    }


    public static bool EqualsIgnoreCase(this string me, string value)
    {
      return StringComparer.InvariantCultureIgnoreCase.Equals(me, value);
    }

    /// <summary>
    /// returns a string with the first letter capitalized. 
    /// </summary>
    public static string FirstCharUpper(this string aString)
    {
      char[] chars = aString.ToCharArray();
      if (chars.Length > 0)
      {
        chars[0] = Char.ToUpper(chars[0]);
      }
      return new string(chars);
    }

    /// <summary>
    /// returns a string with the first letter lower case.
    /// </summary>
    /// <param name="aString"></param>
    /// <returns></returns>
    public static string FirstCharLower(this string aString)
    {
      char[] chars = aString.ToCharArray();
      if (chars.Length > 0)
      {
        chars[0] = Char.ToLower(chars[0]);
      }
      return new string(chars);
    }

    public static string CamelCaseToUpperUnderscored(this string aString)
    {
      return string.Concat(aString.Select((x, i) => (i > 0 && char.IsUpper(x) ? "_" : "") + x)).ToUpper();
    }

    public static string RemoveTrailing(this string aString, string toRemove)
    {
      if (aString.EndsWith(toRemove))
      {
        return aString.Substring(0, aString.Length - toRemove.Length);
      }
      else
      {
        return aString;
      }
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


    public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, Func<TValue> valueFactory)
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
      foreach(Match match in matches)
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
          for(int i=0; i < chars.Length;i++)
          {
            chars[i] = (char)(char9 - chars[i] + char0);
          }
          sb.Append(chars);

          // Check if we have a decimal point in this negative number, and if so then subtract the digits from 9 and pad out to max digits
          int fractionDigitsAdded = 0;
          if (lastStartPos < input.Length && input[lastStartPos]=='.')
          {
            sb.Append('.');
            lastStartPos++;
            while(lastStartPos < input.Length && input[lastStartPos] >= char0 && input[lastStartPos] <= char9)
            {
              sb.Append((char)(char9 - input[lastStartPos] + char0));
              lastStartPos++;
              fractionDigitsAdded++;
            }
            if(fractionDigitsAdded < maxDigits)
            {
              sb.Append('9', maxDigits- fractionDigitsAdded);
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

    public static void AddRange<TKey, TValue>(this IDictionary<TKey,TValue> collection, IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
       source.ForEach(x => collection.Add(x.Key, x.Value));
    }


    //-------------------------------------------------------------------------
    // Returns a hash value for the string, taken from http://stackoverflow.com/a/8317670,
    // same piece of code is used in FlewseLib::StringFns to make the hash value
    // stored as SDW IdCode value
    public static uint FlewseHash(this string str)
    {
      uint b = 378551;
      uint a = 63689;
      uint hash = 0;

      for (int i = 0; i < str.Length; i++)
      {
        hash = hash * a + str[i];
        a = a * b;
      }

      return (hash & 0x7FFFFFFF);
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
    /// Modified version of http://stackoverflow.com/questions/808104/engineering-notation-in-c
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToEngineeringExponent(this double value)
    {
      double absValue = Math.Abs(value);

      if (absValue < 1000 && absValue >0.001)
      {
        return value.ToString("0.###");
      }

      double log10 = Math.Log10(absValue);

      int exp = (int)(Math.Floor(log10 / 3.0) * 3.0);
      double newValue = value * Math.Pow(10.0, -exp);
      while(newValue >= 1000)
      {
        newValue *= 0.001;
        exp += 3;
      }
      return string.Format("{0:0.###}e{1}", newValue, exp);
    }

    /// <summary>
    /// Returns the specified value campled to be within the min/max specified (inclusive)
    /// Checks and fixes min/max being the wrong way around.
    /// </summary>
    /// <param name="val">The value to be clamped</param>
    /// <param name="min">Minimum return value</param>
    /// <param name="max">Maximum return value</param>
    /// <returns></returns>
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
      if (min.CompareTo(max) > 0)
      {
        T tmp = min;
        min = max;
        max = tmp;
      }

      if (val.CompareTo(min) < 0) return min;
      else if (val.CompareTo(max) > 0) return max;
      else return val;
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

    public static string ToCsv<TSource>(this IEnumerable<TSource> source)
    {
      return source.Select(s => s.ToString()).Aggregate((a, b) => $"{a},{b}");
    }

    public static IEnumerable<int> ConcatNegatives(this IEnumerable<int> source)
    {
      return source.Concat(source.Where(s=>s!=0).Select(s => -s));
    }
    public static IEnumerable<float> ConcatNegatives(this IEnumerable<float> source)
    {
      return source.Concat(source.Where(s => s != 0).Select(s => -s));
    }
    public static IEnumerable<double> ConcatNegatives(this IEnumerable<double> source)
    {
      return source.Concat(source.Where(s => s != 0).Select(s => -s));
    }

    public static string RemoveNewLineCharacters(this string _txt)
    {
      return _txt.Replace("\n", " ").Replace("\r", "");
    }
  }
}