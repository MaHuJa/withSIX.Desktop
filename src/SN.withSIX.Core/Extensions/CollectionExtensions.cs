// <copyright company="SIX Networks GmbH" file="CollectionExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq;
using ReactiveUI;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool Matches(this string @this, string pattern) {
            return Regex.IsMatch(@this, pattern);
        }

        public static string Truncate(this string @this, int limit) {
            if (@this.Length <= limit)
                return @this;
            return @this.Substring(0, limit - 3) + "...";
        }

        public static string TruncateNullSafe(this string @this, int limit) {
            if (@this == null)
                return null;
            return @this.Truncate(limit);
        }
    }

    public static class CollectionExtensions
    {
        /// <summary>
        ///     Probably doing it wrong (TM) - why not just replace the whole collection?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="col"></param>
        /// <param name="replacement"></param>
        public static void Replace<T>(this ICollection<T> col, IEnumerable<T> replacement) {
            col.Clear();
            col.AddRange(replacement);
        }

        public static void Replace<T>(this IReactiveList<T> col, IEnumerable<T> replacement)
        {
            col.Clear();
            col.AddRange(replacement);
        }

        public static T Find<T, TId>(this IEnumerable<T> col, TId id) where T : IHaveId<TId> {
            return col.FirstOrDefault(x => id.Equals(x.Id));
        }

        public static T FindOrThrow<T, TId>(this IEnumerable<T> col, TId id) where T : IHaveId<TId> {
            var item = col.FirstOrDefault(x => id.Equals(x.Id));
            if (item == null)
                throw new NotFoundException("Item with ID not found: " + id);
            return item;
        }


        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T item) {
            return enumerable.Concat(Enumerable.Repeat(item, 1));
        }

        public static async Task<IEnumerable<TOut>> SelectAsync<TIn, TOut>(this IEnumerable<TIn> input,
            Func<TIn, Task<TOut>> doFunc) {
            var list = new List<TOut>();
            foreach (var t in input)
                list.Add(await doFunc(t).ConfigureAwait(false));
            return list;
        }

        public static bool HasSameElements<T>(this IEnumerable<T> list1, IEnumerable<T> list2) {
            return new HashSet<T>(list1).SetEquals(list2);
        }

        public static T RandomElement<T>(this IList<T> list) {
            var random = new Random();
            return list[random.Next(0, list.Count)];
        }

        public static T UpdateOrAdd<T>(this IList<T> list, T item, bool reverse = false, string[] extraExclusions = null)
            where T : class, IComparePK<T> {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(item != null);

            T found;

            lock (list) {
                found = list.ToArray().FirstOrDefault(x => x.ComparePK(item));
                if (found == null) {
                    if (reverse)
                        list.Insert(0, item);
                    else
                        list.Add(item);

                    return item;
                }
            }

            item.CopyProperties(found, extraExclusions);
            return found;
        }

        public static T[] UpdateOrAdd<T>(this IList<T> list, IEnumerable<T> items, bool reverse = false,
            string[] extraExclusions = null)
            where T : class, IComparePK<T> {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);

            T[] existingItems;
            T[] newItems;
            var itemsArray = items.ToArray();
            lock (list) {
                var array = list.ToArray();
                existingItems = itemsArray.Select(x => array.FirstOrDefault(x.ComparePK))
                    .Where(x => x != null)
                    .ToArray();

                newItems = itemsArray.Where(x => existingItems.None(x.ComparePK))
                    .ToArray();

                list.AddRange(newItems, reverse);
            }

            foreach (var i in existingItems) {
                var item = itemsArray.First(y => y.ComparePK(i));
                item.CopyProperties(i, extraExclusions);
            }

            return newItems;
        }

        public static void SyncCollection<T>(this IEnumerable<T> source, IList<T> destination,
            Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            if (filter != null)
                source = source.Where(filter);

            var srcAr = source.ToArray();
            destination.RemoveRange(destination.Except(srcAr).ToArray());
            destination.AddRange(srcAr.Except(destination).ToArray());
        }

        public static void ImportCollection<T>(this IList<T> destination, IEnumerable<T> source) {
            source.SyncCollection(destination);
        }

        public static void ImportCollection<T>(this IList<T> destination, IEnumerable<T> source,
            Func<T, bool> filter) {
            Contract.Requires<ArgumentNullException>(filter != null);
            source.SyncCollection(destination, filter);
        }

        public static void SyncCollection2<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, bool> filter = null) where T : T2 {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            if (filter != null)
                source = source.Where(filter);

            var srcAr = source.ToArray();
            destination.RemoveRange(destination.Except(srcAr.Cast<T2>()).ToArray());
            destination.AddRange(srcAr.Cast<T2>().Except(destination).ToArray());
        }

        public static void SyncCollectionOfType<T, T2>(this IEnumerable<T2> source, IList<T2> destination,
            Func<T, bool> filter = null) where T : T2 {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            var src = source.OfType<T>();

            if (filter != null)
                src = src.Where(filter);

            var srcAr = src.Cast<T2>().ToArray();
            destination.RemoveRange(destination.Where(x => x is T).Except(srcAr).ToArray());
            destination.AddRange(srcAr.Except(destination).ToArray());
        }

        public static void SyncCollectionOfType<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, bool> filter = null) where T : T2 {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            var src = source;
            if (filter != null)
                src = src.Where(filter);

            var srcAr = src.Cast<T2>().ToArray();
            destination.RemoveRange(destination.Where(x => x is T).Except(srcAr).ToArray());
            destination.AddRange(srcAr.Except(destination).ToArray());
        }

        public static void SyncCollectionOfTypeLocked<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, bool> filter = null) where T : T2 {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            var src = source;
            if (filter != null)
                src = src.Where(filter);

            var srcAr = src.Cast<T2>().ToArray();
            lock (destination) {
                destination.RemoveRange(destination.Where(x => x is T).Except(srcAr).ToArray());
                destination.AddRange(srcAr.Except(destination).ToArray());
            }
        }

        public static void SyncCollectionPK<T>(this IEnumerable<T> source, IList<T> destination,
            Func<T, bool> filter = null) where T : class, IComparePK<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            if (filter != null)
                source = source.Where(filter);

            var srcAr = source.ToArray();
            destination.RemoveRange(destination.Where(x => srcAr.None(x.ComparePK)).ToArray());
            destination.UpdateOrAdd(srcAr);
        }

        public static void AddLocked<T>(this ICollection<T> collection, T item) {
            lock (collection)
                collection.Add(item);
        }

        public static void SyncCollectionLocked<T>(this IEnumerable<T> source, IList<T> destination,
            Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            lock (destination)
                SyncCollection(source, destination, filter);
        }

        public static void SyncCollectionConvert<T, T2>(this IEnumerable<T> source, IList<T2> destination)
            where T2 : IHaveModel<T> where T : IComparePK<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            SyncCollectionConvertCustomPK(source.ToArray(), destination,
                x => (T2) Activator.CreateInstance(typeof (T2), x), x => true);
        }

        public static void SyncCollectionConvertLocked<T, T2>(this IEnumerable<T> source, IList<T2> destination)
            where T2 : IHaveModel<T> where T : IComparePK<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            lock (destination)
                SyncCollectionConvert(source, destination);
        }

        public static void SyncCollectionConvertCustomPK<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, T2> func, Func<T2, bool> filter)
            where T2 : IHaveModel<T> where T : IComparePK<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            var srcAr = source.ToArray();
            destination.RemoveRange(destination.Where(x => filter(x) && srcAr.None(y => x.Model.ComparePK(y))).ToArray());
            destination.AddRange(srcAr.Except(destination.Select(x => x.Model)).Select(func).ToArray());
        }

        public static void SyncCollectionConvertCustomLockedPK<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, T2> func, Func<T2, bool> filter) where T2 : IHaveModel<T> where T : IComparePK<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            lock (destination)
                SyncCollectionConvertCustomPK(source, destination, func, filter);
        }

        public static void SyncCollectionConvertCustom<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, T2> func, Func<T2, bool> filter)
            where T2 : IHaveModel<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            var srcAr = source.ToArray();
            destination.RemoveRange(destination.Where(x => filter(x) && srcAr.None(y => x.Model.Equals(y))).ToArray());
            destination.AddRange(srcAr.Except(destination.Select(x => x.Model)).Select(func).ToArray());
        }

        public static void SyncCollectionConvertCustomLocked<T, T2>(this IEnumerable<T> source, IList<T2> destination,
            Func<T, T2> func, Func<T2, bool> filter)
            where T2 : IHaveModel<T> where T : IComparePK<T> {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(destination != null);

            lock (destination)
                SyncCollectionConvertCustom(source, destination, func, filter);
        }

        public static void AddWhenMissing<T>(this IList<T> list, T item, bool reverse = false) where T : class {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(item != null);

            lock (list) {
                if (!list.None(item.Equals))
                    return;

                if (reverse)
                    list.Insert(0, item);
                else
                    list.Add(item);
            }
        }

        public static void AddWhenMissing(this IList<string> list, string item, StringComparison comparer,
            bool reverse = false) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(item != null);
            lock (list) {
                if (!list.None(x => item.Equals(x, comparer)))
                    return;

                if (reverse)
                    list.Insert(0, item);
                else
                    list.Add(item);
            }
        }

        public static void ReAdd<T>(this IList<T> list, T item, bool reverse = false) where T : class {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(item != null);

            lock (list) {
                list.Remove(item);

                if (reverse)
                    list.Insert(0, item);
                else
                    list.Add(item);
            }
        }

        public static void AddWhenMissing<T>(this IList<T> list, IEnumerable<T> items, bool reverse = false) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            lock (list) {
                var missingItems = items.Where(i => list.None(j => j.Equals(i))).ToArray();
                list.AddRange(missingItems, reverse);
            }
        }

        public static void AddWhenMissing(this IList<string> list, IEnumerable<string> items, StringComparison comparer,
            bool reverse = false) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            lock (list) {
                var missingItems = items.Where(i => list.None(j => j.Equals(i, comparer))).ToArray();
                list.AddRange(missingItems, reverse);
            }
        }

        public static void AddWhenMissing<T, T2>(this IDictionary<T, T2> list, IDictionary<T, T2> items,
            bool reverse = false) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            lock (list) {
                var missingItems = items.Where(i => list.None(j => j.Key.Equals(i.Key)))
                    .ToDictionary(x => x.Key, x => x.Value);
                list.AddRange(missingItems, reverse);
            }
        }

        public static void AddWhenMissingPK<T>(this IList<T> list, T item, bool reverse = false)
            where T : class, IComparePK<T> {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(item != null);
            lock (list) {
                if (!list.None(item.ComparePK))
                    return;

                if (reverse)
                    list.Insert(0, item);
                else
                    list.Add(item);
            }
        }

        public static void AddWhenMissingPK<T>(this IList<T> list, IEnumerable<T> items, bool reverse = false)
            where T : IComparePK<T> {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            lock (list) {
                var missingItems = items.Where(i => list.None(i.ComparePK)).ToArray();
                list.AddRange(missingItems, reverse);
            }
        }

        public static T[] ToArrayLocked<T>(this IEnumerable<T> list) {
            Contract.Requires<ArgumentNullException>(list != null);
            lock (list)
                return list.ToArray();
        }

        public static void RemoveRange<T>(this IList<T> list, ICollection<T> items) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            var l = list as List<T>;
            if (l != null)
                // This is the reason why we take items as ICollection - otherwise it would re-iterate the items all the time.
                l.RemoveAll(items.Contains);
            else {
                var l2 = list as ReactiveList<T>;
                if (l2 != null)
                    l2.RemoveAll(items);
                else
                    items.ForEach(x => list.Remove(x));
            }
        }

        public static void RemoveRangeLocked<T>(this IList<T> list, ICollection<T> items) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            lock (list) {
                list.RemoveRange(items);
            }
        }

        public static void AddRange<T, T2>(this IDictionary<T, T2> list, IDictionary<T, T2> items, bool reverse = false) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            foreach (var i in (reverse ? items.Reverse() : items))
                list.Add(i.Key, i.Value);
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items, bool reverse = false) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            var list3 = list as List<T>;
            if (list3 != null)
                list3.AddRange(items);
            else {
                var list2 = list as IReactiveList<T>;
                if (list2 != null)
                    list2.AddRange(items);
                else {
                    foreach (var i in items) {
                        if (reverse)
                            list.Insert(0, i);
                        else
                            list.Add(i);
                    }
                }
            }
        }

        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            var list3 = list as List<T>;
            if (list3 != null)
                list3.AddRange(items);
            else {
                var list2 = list as IReactiveList<T>;
                if (list2 != null)
                    list2.AddRange(items);
                else {
                    foreach (var i in items)
                        list.Add(i);
                }
            }
        }

        public static void RemoveAll<T>(this ICollection<T> list, IEnumerable<T> items) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);
            var list3 = list as List<T>;
            if (list3 != null)
                list3.RemoveAll(items.Contains);
            else {
                var list2 = list as IReactiveList<T>;
                if (list2 != null)
                    list2.RemoveAll(items);
                else {
                    foreach (var i in items)
                        list.Remove(i);
                }
            }
        }

        public static void RemoveAll<T>(this IReactiveList<T> list, Predicate<T> predicate) {
            list.RemoveAll(list.Where(x => predicate(x)).ToArray());
        }

        public static void RemoveAll<T, T2>(this List<T> list, Predicate<T2> predicate) where T2 : T {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.RemoveAll(x => predicate((T2) x));
        }

        public static void RemoveFirst<T>(this IList<T> list, Predicate<T> predicate) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.Remove(list.FirstOrDefault(x => predicate(x)));
        }

        public static void RemoveFirst<T, T2>(this IList<T> list, Predicate<T2> predicate) where T2 : T {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.Remove(list.Cast<T2>().FirstOrDefault(x => predicate(x)));
        }

        public static void RemoveSingle<T>(this IList<T> list, Predicate<T> predicate) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.Remove(list.SingleOrDefault(x => predicate(x)));
        }

        public static void RemoveSingle<T, T2>(this IList<T> list, Predicate<T2> predicate) where T2 : T {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.Remove(list.Cast<T2>().SingleOrDefault(x => predicate(x)));
        }

        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action) {
            Contract.Requires<ArgumentNullException>(ie != null);
            Contract.Requires<ArgumentNullException>(action != null);
            var i = 0;
            foreach (var e in ie)
                action(e, i++);
        }
    }
}