// <copyright company="SIX Networks GmbH" file="DefaultDomainEventHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications.Extensions;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    public class DefaultDomainEventHandler : IDomainEventHandler
    {
        readonly ConcurrentDictionary<object, List<IDomainEvent>> _handlers =
            new ConcurrentDictionary<object, List<IDomainEvent>>();

        public void PrepareEvent(object obj, IDomainEvent evt) {
            var list = GetList(obj);
            list.Add(evt);
        }

        public async Task RaiseEvents() {
            foreach (var obj in _handlers.Select(x => x.Key))
                await RaiseEvents(obj).ConfigureAwait(false);
        }

        async Task RaiseEvents(object obj) {
            List<IDomainEvent> list;
            if (_handlers.TryRemove(obj, out list))
                await list.RaiseEvents().ConfigureAwait(false);
        }

        List<IDomainEvent> GetList(object obj) {
            return _handlers.GetOrAdd(obj, reference => new List<IDomainEvent>());
        }
    }
}