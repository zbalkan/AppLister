﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryEngine.Extensions
{
    internal static class CollectionExtensions
    {
        /// <summary>
        ///     Recursively select all subitems based on the selector.
        /// </summary>
        internal static IEnumerable<T> SelectManyResursively<T>(this IEnumerable<T> enumerable, Func<T, IEnumerable<T>> subitemSelector) => enumerable.SelectMany(
                x => Enumerable.Repeat(x, 1)
                .Concat(subitemSelector(x).SelectManyResursively(subitemSelector)));

        /// <summary>
        ///     Move item at specified index to a new index
        /// </summary>
        internal static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }

        /// <summary>
        ///     Run distinct using the specified equality comparator
        /// </summary>
        internal static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source,
            Func<TSource, TSource, bool> equalityComparator)
        {
            var seenItems = new List<TSource>();
            foreach (var item in source)
            {
                if (seenItems.Any(x => equalityComparator(item, x)))
                {
                    continue;
                }

                seenItems.Add(item);
                yield return item;
            }
        }

        /// <summary>
        ///     Run the specified action on all members of the collection as they are enumerated.
        ///     Action will be executed for each enumeration over the element (lazy evaluation).
        /// </summary>
        /// <typeparam name="T">
        ///     Type that is being iterated over
        /// </typeparam>
        /// <param name="collection">
        ///     Base enumerable
        /// </param>
        /// <param name="action">
        ///     Action to run on all of the enumerated members
        /// </param>
        /// <returns>
        ///     Enumerator
        /// </returns>
        internal static IEnumerable<T> DoForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        ///     Remove all items that are contained in the supplied collection.
        /// </summary>
        /// <param name="collection">
        ///     Collection to remove items from
        /// </param>
        /// <param name="items">
        ///     Collection with items to remove.
        /// </param>
        internal static void RemoveAll<T>(this IList<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (collection.Contains(item))
                {
                    collection.Remove(item);
                }
            }
        }

        /// <summary>
        ///     Check if any element of the collection equals to the supplied string value.
        /// </summary>
        internal static bool Contains(this IEnumerable<string> data, string value, StringComparison options) => data.Any(x => x.Equals(value, options));
    }
}