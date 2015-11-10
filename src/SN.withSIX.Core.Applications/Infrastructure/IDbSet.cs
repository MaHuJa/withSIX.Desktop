// <copyright company="SIX Networks GmbH" file="IDbSet.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using ReactiveUI;
using SN.withSIX.Api.Models.Exceptions;

namespace SN.withSIX.Core.Applications.Infrastructure
{
    public interface IDbSet<TEntity, in TId> : IQueryable<TEntity> where TEntity : IHaveId<TId>
    {
        ReactiveList<TEntity> Local { get; }
        TEntity Find(TId id);
    }

    public static class DbSetExtensions
    {
        public static TEntity FindOrThrow<TEntity, TId>(this IDbSet<TEntity, TId> dbSet, TId id)
            where TEntity : IHaveId<TId> {
            var item = dbSet.Find(id);
            if (item == null)
                throw new NotFoundException("Entity not found with key: " + id);
            return item;
        }
    }
}