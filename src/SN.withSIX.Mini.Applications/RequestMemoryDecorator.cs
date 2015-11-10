// <copyright company="SIX Networks GmbH" file="RequestMemoryDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Mini.Applications
{
    public class RequestMemoryDecorator : IMediator
    {
        readonly IMediator _target;

        public RequestMemoryDecorator(IMediator target) {
            _target = target;
        }

        public TResponseData Request<TResponseData>(IRequest<TResponseData> request) {
            try {
                return _target.Request(request);
            } finally {
                Collect();
            }
        }

        public async Task<TResponseData> RequestAsync<TResponseData>(IAsyncRequest<TResponseData> request) {
            try {
                return await _target.RequestAsync(request).ConfigureAwait(false);
            } finally {
                Collect();
            }
        }

        public void Notify<TNotification>(TNotification notification) {
            _target.Notify(notification);
            Collect();
        }

        public async Task NotifyAsync<TNotification>(TNotification notification) {
            await _target.NotifyAsync(notification).ConfigureAwait(false);
            Collect();
        }

        static void Collect() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}