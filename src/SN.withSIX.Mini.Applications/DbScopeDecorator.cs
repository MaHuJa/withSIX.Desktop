// <copyright company="SIX Networks GmbH" file="DbScopeDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications
{
    // TODO: We should auto save?!
    public class DbScopeDecorator : IMediator
    {
        readonly IDbContextFactory _factory;
        readonly IMediator _target;

        public DbScopeDecorator(IMediator target, IDbContextFactory factory) {
            _target = target;
            _factory = factory;
        }

        public TResponseData Request<TResponseData>(IRequest<TResponseData> request) {
            using (_factory.Create())
                return _target.Request(request);
        }

        public async Task<TResponseData> RequestAsync<TResponseData>(IAsyncRequest<TResponseData> request) {
            using (_factory.Create())
                return await _target.RequestAsync(request).ConfigureAwait(false);
        }

        public void Notify<TNotification>(TNotification notification) {
            using (_factory.Create())
                _target.Notify(notification);
        }

        public async Task NotifyAsync<TNotification>(TNotification notification) {
            using (_factory.Create())
                await _target.NotifyAsync(notification).ConfigureAwait(false);
        }
    }
}