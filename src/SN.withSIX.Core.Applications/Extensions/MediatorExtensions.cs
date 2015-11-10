// <copyright company="SIX Networks GmbH" file="MediatorExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.Extensions
{
    public static class MediatorExtensions
    {
        /// <summary>
        ///     Wrapped into a Task.Run, so that all processing of the command happens on the background thread.
        /// </summary>
        /// <typeparam name="TResponseData"></typeparam>
        /// <param name="mediator"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Task<TResponseData> RequestAsyncWrapped<TResponseData>(this IMediator mediator,
            IAsyncRequest<TResponseData> request) {
            return Task.Run(() => mediator.RequestAsync(request));
        }

        public static Task<TResponseData> RequestAsync<TResponseData>(this IMediator mediator,
            ICompositeCommand<TResponseData> message) {
            return message.Execute(mediator);
        }
    }
}