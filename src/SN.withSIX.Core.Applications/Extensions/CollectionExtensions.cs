// <copyright company="SIX Networks GmbH" file="CollectionExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Applications.Extensions
{
    public static class CollectionExtensions
    {
        public static Task<T> FirstAsync<T>(this IEnumerable<T> col) {
            return Task.Run(() => col.First());
        }

        public static Task<T> FirstAsync<T>(this IEnumerable<T> col, Func<T, bool> predicate) {
            return Task.Run(() => col.First(predicate));
        }

        public static Task<T> LastAsync<T>(this IEnumerable<T> col, Func<T, bool> predicate) {
            return Task.Run(() => col.Last(predicate));
        }

        public static Task<T> LastAsync<T>(this IEnumerable<T> col) {
            return Task.Run(() => col.Last());
        }

        public static Task<T> LastOrDefaultAsync<T>(this IEnumerable<T> col) {
            return Task.Run(() => col.LastOrDefault());
        }

        public static Task<T> FindFromRequestAsync<T, TId>(this IEnumerable<T> col, IHaveId<TId> idBearer)
            where T : IHaveId<TId> {
            return Task.Run(() => col.FindFromRequest(idBearer));
        }

        public static Task<T> FindAsync<T, TId>(this IEnumerable<T> col, TId id) where T : IHaveId<TId> {
            return Task.Run(() => col.Find(id));
        }

        public static T FindFromRequest<T, TId>(this IEnumerable<T> col, IHaveId<TId> idBearer) where T : IHaveId<TId> {
            return col.FirstOrDefault(x => idBearer.Id.Equals(x.Id));
        }

        public static Task<T> FindOrThrowFromRequestAsync<T, TId>(this IEnumerable<T> col, IHaveId<TId> idBearer)
            where T : IHaveId<TId> {
            return Task.Run(() => col.FindOrThrowFromRequest(idBearer));
        }

        public static Task<T> FindOrThrowAsync<T, TId>(this IEnumerable<T> col, TId id) where T : IHaveId<TId> {
            return Task.Run(() => col.FindOrThrow(id));
        }

        public static T FindOrThrowFromRequest<T, TId>(this IEnumerable<T> col, IHaveId<TId> idBearer)
            where T : IHaveId<TId> {
            var item = col.FirstOrDefault(x => idBearer.Id.Equals(x.Id));
            if (item == null)
                throw new NotFoundException("Item with ID not found: " + idBearer.Id);
            return item;
        }

        public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> col) {
            return Task.Run(() => col.ToList());
        }
    }
}