// <copyright company="SIX Networks GmbH" file="GameContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    // TODO: Consider to use an alternative underlying storage mechanism: Key Value, so we can ignore games who's plugins are currently not installed!

    // What about data migration over time? Best to go Entity Framework directly with migrations, or stick to Akavache basic K/V store and do manual plumbing??
    // TODO: Future - Upgrade to a system where we are not serializing/deserializing the whole data store at once ;-)
    public abstract class GameContext : IGameContext
    {
        readonly ICollection<Action> _transactionCallbacks = new Collection<Action>();
        readonly ICollection<Func<Task>> _transactionCallbacksAsync = new Collection<Func<Task>>();
        public virtual ICollection<Game> Games { get; protected set; } = new List<Game>();
        public abstract Task Load(Guid gameId);
        public abstract Task LoadAll(bool skip = false);
        public virtual IDomainEventHandler DomainEventHandler { get; } = new DefaultDomainEventHandler();
        // Instead we use Games as the aggregate root and therefore also spare us some Persistence plumbing right now...
        //        public virtual ICollection<Content> Contents { get; } = new List<Content>();
        //        public virtual ICollection<RecentItem> Recents { get; } = new List<RecentItem>();

        public async Task<int> SaveChanges() {
            await RaiseEvents().ConfigureAwait(false);
            var changes = await SaveChangesInternal().ConfigureAwait(false);
            await ExecuteTransactionCallbacks().ConfigureAwait(false);
            return changes;
        }

        public void AddTransactionCallback(Action act) {
            _transactionCallbacks.Add(act);
        }

        public void AddTransactionCallback(Func<Task> act) {
            _transactionCallbacksAsync.Add(act);
        }

        public abstract Task Migrate();

        // We expect a convention where the settings exist in the same namespace as the game, and are {GameClassName}Settings

        protected abstract Task<int> SaveChangesInternal();

        async Task ExecuteTransactionCallbacks() {
            var callbacks = _transactionCallbacks.ToArray();
            _transactionCallbacks.Clear();

            var asyncCallbacks = _transactionCallbacksAsync.ToArray();
            _transactionCallbacksAsync.Clear();

            foreach (var c in callbacks)
                c();

            foreach (var c in asyncCallbacks)
                await c().ConfigureAwait(false);
        }

        Task RaiseEvents() {
            return DomainEventHandler.RaiseEvents();
        }
    }
}