// <copyright company="SIX Networks GmbH" file="MediatorLoggingDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core.Presentation.Decorators
{
    public abstract class LoggingDecorator
    {
        protected static readonly JsonSerializerSettings JsonSerializerSettings = CreateJsonSerializerSettings();

        static JsonSerializerSettings CreateJsonSerializerSettings() {
            var settings = new JsonSerializerSettings().SetDefaultSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return settings;
        }
    }

    public class MediatorLoggingDecorator : LoggingDecorator, IMediator
    {
        readonly IMediator _mediator;

        public MediatorLoggingDecorator(IMediator mediator) {
            Contract.Requires<ArgumentNullException>(mediator != null);
            _mediator = mediator;
        }

        public TResponseData Request<TResponseData>(IRequest<TResponseData> request) {
            using (
                _mediator.Bench(
                    startMessage:
                        "Writes: " + (request is IWrite) + ", Data: " +
                        JsonConvert.SerializeObject(request, JsonSerializerSettings),
                    caller: "Request" + ": " + request.GetType()))
                return _mediator.Request(request);
        }

        public async Task<TResponseData> RequestAsync<TResponseData>(IAsyncRequest<TResponseData> request) {
            using (_mediator.Bench(
                startMessage:
                    "Writes: " + (request is IWrite) + ", Data: " +
                    JsonConvert.SerializeObject(request, JsonSerializerSettings),
                caller: "RequestAsync" + ": " + request.GetType()))
                return await _mediator.RequestAsync(request).ConfigureAwait(false);
        }

        // We don't log the notification object because notifications can contain complex objects and huge hierarchies..
        public void Notify<TNotification>(TNotification notification) {
            using (_mediator.Bench(caller: "Notify" + ": " + notification.GetType()))
                _mediator.Notify(notification);
        }

        public async Task NotifyAsync<TNotification>(TNotification notification) {
            using (_mediator.Bench(caller: "NotifyAsync" + ": " + notification.GetType()))
                await _mediator.NotifyAsync(notification).ConfigureAwait(false);
        }
    }
}