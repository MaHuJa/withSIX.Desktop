// <copyright company="SIX Networks GmbH" file="EventExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Applications.Extensions
{
    public static class EventExtensions
    {
        public static async Task RaiseEvents(this IEnumerable<IDomainEvent> events) {
            foreach (var evt in events)
                await evt.RaiseEvent().ConfigureAwait(false);
        }

        public static Task RaiseEvent(this IDomainEvent evt) {
            // Dynamic to fix the generic type use in the mediator...
            dynamic notification = evt;
            return Raise(notification);
        }

        static Task Raise<TMessage>(TMessage message) {
            return message.Raise();
        }
    }
}