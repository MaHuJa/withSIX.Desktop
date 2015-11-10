// <copyright company="SIX Networks GmbH" file="ReactiveUIExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Extensions
{
    public static class ReactiveUIExtensions
    {
        public static void CopyAndTrackChangesOnUiThread<T>(this IReactiveList<T> list, Action<T> add, Action<T> remove,
            Action<IEnumerable<T>> reset, Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(list != null);
            reset(list);
            TrackChangesOnUiThread(list, add, remove, reset, filter);
        }

        public static CompositeDisposable KeepCollectionInSync<T>(this IReactiveList<T> list, IList<T> destination,
            Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(destination != null);
            lock (list) {
                list.SyncCollection(destination, filter);
                return list.TrackChanges(destination.Add, x => destination.Remove(x),
                    reset => reset.SyncCollection(destination), filter);
            }
        }

        public static CompositeDisposable KeepCollectionInSync2<T, T2>(this IReactiveList<T> list, IList<T2> destination,
            Func<T, bool> filter = null) where T : T2 {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(destination != null);
            lock (list) {
                list.SyncCollection2(destination, filter);
                return list.TrackChanges(x => destination.Add(x), x => destination.Remove(x),
                    reset => reset.SyncCollection2(destination), filter);
            }
        }

        public static CompositeDisposable KeepCollectionInSyncOfType<T, T2>(this IReactiveList<T2> list,
            IList<T2> destination, Func<T, bool> filter = null)
            where T : class, T2 where T2 : class {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(destination != null);
            lock (list) {
                list.SyncCollectionOfType(destination, filter);
                return list.TrackChangesOfType(destination.Add, x => destination.Remove(x),
                    reset => reset.SyncCollectionOfType<T, T2>(destination), filter);
            }
        }

        public static CompositeDisposable KeepCollectionInSyncOfType<T, T2>(this IReactiveList<T> list,
            IList<T2> destination, Func<T, bool> filter = null)
            where T : class, T2
            where T2 : class {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(destination != null);
            lock (list) {
                list.SyncCollectionOfType(destination, filter);
                return list.TrackChangesOfType(destination.Add, x => destination.Remove(x),
                    reset => reset.SyncCollectionOfType(destination), filter);
            }
        }

        public static CompositeDisposable TrackChanges<T>(this IReactiveList<T> list, Action<T> add, Action<T> remove,
            Action<ICollection<T>> reset, Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(list != null);
            var disposables = new CompositeDisposable();
            if (add != null) {
                var o = list.ItemsAdded;
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.Subscribe(add));
            }
            if (remove != null) {
                var o = list.ItemsRemoved;
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.Subscribe(remove));
            }
            if (reset != null) {
                disposables.Add(list.ShouldReset
                    .Subscribe(x => reset(filter == null ? list.ToArray() : list.Where(filter).ToArray())));
            }

            return disposables;
        }

        public static CompositeDisposable TrackChangesBeforeAdded<T>(this IReactiveList<T> list, Action<T> add,
            Action<T> remove,
            Action<IEnumerable<T>> reset, Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(list != null);
            var disposables = new CompositeDisposable();
            if (add != null) {
                var o = list.BeforeItemsAdded;
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.Subscribe(add));
            }
            if (remove != null) {
                var o = list.BeforeItemsRemoved;
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.Subscribe(remove));
            }
            if (reset != null) {
                disposables.Add(list.ShouldReset
                    .Subscribe(x => reset(filter == null ? list.ToArray() : list.Where(filter).ToArray())));
            }

            return disposables;
        }

        public static CompositeDisposable TrackChangesOnUiThread<T>(this IReactiveList<T> list, Action<T> add,
            Action<T> remove,
            Action<IEnumerable<T>> reset, Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(list != null);
            var disposables = new CompositeDisposable();
            if (add != null) {
                var o = list.ItemsAdded;
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(add));
            }
            if (remove != null) {
                var o = list.ItemsRemoved;
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(remove));
            }
            if (reset != null) {
                disposables.Add(list.ShouldReset
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => reset(filter == null ? list.ToArray() : list.Where(filter).ToArray())));
            }

            return disposables;
        }

        public static CompositeDisposable TrackChangesOfType<T, T2>(this IReactiveList<T2> list, Action<T2> add,
            Action<T2> remove,
            Action<IEnumerable<T2>> reset, Func<T, bool> filter = null)
            where T2 : class
            where T : class, T2 {
            Contract.Requires<ArgumentNullException>(list != null);

            var disposables = new CompositeDisposable();
            if (add != null) {
                var o = list.ItemsAdded.OfType<T>();
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.Subscribe(add));
            }
            if (remove != null) {
                var o = list.ItemsRemoved.OfType<T>();
                if (filter != null)
                    o = o.Where(filter);
                disposables.Add(o.Subscribe(remove));
            }
            if (reset != null) {
                disposables.Add(list.ShouldReset
                    .Subscribe(
                        x =>
                            reset(filter == null
                                ? list.OfType<T>().ToArray()
                                : list.OfType<T>().Where(filter).ToArray())));
            }
            return disposables;
        }

        public static CompositeDisposable TrackChangesDerrivedConvert<T, T2>(this IReactiveList<T> list,
            IList<T2> target,
            Func<T, T2> convert, Func<T, bool> filter = null) where T2 : class {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(target != null);
            return list.TrackChanges(x => target.AddWhenMissing(convert(x)), x => target.Remove(convert(x)), l => {
                target.Clear();
                target.AddRange(l.Select(convert));
            }, filter);
        }

        public static CompositeDisposable TrackChangesDerrived<T>(this IReactiveList<T> list, IList<T> target,
            Func<T, bool> filter = null) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(target != null);
            return list.TrackChanges(target.Add, x => target.Remove(x),
                reset => reset.SyncCollectionLocked(target), filter);
        }

        public static T UpdateOrAdd<T, T2>(this IHaveItems<T, T2> @this, T item, string[] extraExclusions = null)
            where T : class, IComparePK<T>
            where T2 : IList<T> {
            Contract.Requires<ArgumentNullException>(@this != null);
            Contract.Requires<ArgumentNullException>(item != null);
            return @this.Items.UpdateOrAdd(item, false, extraExclusions);
        }

        public static T[] UpdateOrAdd<T, T2>(this IHaveItems<T, T2> @this, IEnumerable<T> items,
            string[] extraExclusions = null)
            where T : class, IComparePK<T>
            where T2 : IList<T> {
            Contract.Requires<ArgumentNullException>(@this != null);
            Contract.Requires<ArgumentNullException>(items != null);
            return @this.Items.UpdateOrAdd(items, false, extraExclusions);
        }

        public static void RemoveAll<T>(this ReactiveList<T> list, Predicate<T> predicate) {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.RemoveAll(list.Where(x => predicate(x)).ToArray());
        }

        public static void RemoveAll<T, T2>(this ReactiveList<T> list, Predicate<T2> predicate) where T2 : T {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            list.RemoveAll(list.Where(x => predicate((T2) x)).ToArray());
        }
    }
}