using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace InventoryEngine.Extensions
{
    internal static class EnumerableExtensions
    {
        private const int DefaultInternalSetCapacity = 7;

        /// <summary>
        ///     Select using the given action, but ignore exceptions and skip offending items.
        /// </summary>
        internal static IEnumerable<TOut> Attempt<TIn, TOut>(this IEnumerable<TIn> baseEnumerable,
            Func<TIn, TOut> action)
        {
            foreach (var item in baseEnumerable)
            {
                TOut output;
                try
                {
                    output = action(item);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Attempt failed, skipping. Error: " + e);
                    continue;
                }
                yield return output;
            }
        }

        /// <summary>
        ///     Returns distinct elements from a sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of key to distinguish elements by.
        /// </typeparam>
        /// <param name="source">
        ///     The sequence to remove duplicate elements from.
        /// </param>
        /// <param name="keySelector">
        ///     A function to extract the key for each element.
        /// </param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}" /> that contains distinct elements from the source sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         This method is implemented by using deferred execution. The immediate return
        ///         value is an object that stores all the information that is required to perform
        ///         the action. The query represented by this method is not executed until the
        ///         object is enumerated either by calling its `GetEnumerator` method directly or by
        ///         using `foreach` in Visual C# or `For Each` in Visual Basic.
        ///     </para>
        ///     <para>
        ///         The
        ///         <see cref="DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        ///         method returns an unordered sequence that contains no duplicate values. The
        ///         default equality comparer, <see cref="EqualityComparer{T}.Default" />, is used
        ///         to compare values.
        ///     </para>
        /// </remarks>
        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            return DistinctByIterator(source, keySelector);
        }

        /// <summary>
        ///     Wraps the specified disposable in an using statement. Dispose is called on the
        ///     current item when the next item is enumerated, at the end of the enumeration, and
        ///     when an uncaught exception is thrown.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the base enumerable
        /// </typeparam>
        /// <typeparam name="TDisp">
        ///     Type of the disposable class
        /// </typeparam>
        /// <param name="baseEnumerable">
        ///     Base enumerable
        /// </param>
        /// <param name="disposableGetter">
        ///     Lambda for getting the disposable
        /// </param>
        internal static IEnumerable<TDisp> Using<T, TDisp>(this IEnumerable<T> baseEnumerable,
            Func<T, TDisp> disposableGetter) where TDisp : class, IDisposable
        {
            TDisp disposable = null;
            try
            {
                foreach (var item in baseEnumerable)
                {
                    disposable?.Dispose();

                    disposable = disposableGetter(item);
                    yield return disposable;
                }
            }
            finally
            {
                disposable?.Dispose();
            }
        }

        // The DistinctBy method is for .Net 6.0 and above.
        // Reference: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Linq/src/System/Linq/Distinct.cs#L48
        private static IEnumerable<TSource> DistinctByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var set = new HashSet<TKey>(DefaultInternalSetCapacity, null);
            do
            {
                var element = enumerator.Current;
                if (set.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
            while (enumerator.MoveNext());
        }
    }
}