// <copyright company="SIX Networks GmbH" file="MediatorExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels;

namespace SN.withSIX.Mini.Applications.Extensions
{
    public static class MediatorExtensions
    {
        public static Task<TResponseData> Execute<TResponseData>(this ICompositeCommand<TResponseData> message) {
            return message.Execute(Cheat.Mediator);
        }

        public static Task<TResponseData> Execute<TResponseData>(this IAsyncRequest<TResponseData> message) {
            return Cheat.Mediator.RequestAsync(message);
        }

        public static Task Notify<TMessage>(this TMessage message) {
            Cheat.Mediator.Notify(message);
            return Cheat.Mediator.NotifyAsync(message);
        }

        public static async Task Raise<TMessage>(this TMessage message) {
            await message.Notify().ConfigureAwait(false);
            // TODO: Bus messages are supposed to occur AFTER transaction has finished, not before...
            Cheat.MessageBus.SendMessage(message);
        }

        public static Task<T> OpenScreen<T>(this IAsyncQuery<T> query) where T : class, IScreenViewModel {
            return Cheat.ScreenOpener.OpenAsyncQuery(query);
        }

        public static Task<T> OpenScreenCached<T>(this IAsyncQuery<T> query) where T : class, IScreenViewModel {
            return Cheat.ScreenOpener.OpenAsyncQueryCached(query);
        }
    }
}