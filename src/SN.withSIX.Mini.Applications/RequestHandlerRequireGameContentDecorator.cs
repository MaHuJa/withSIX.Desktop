// <copyright company="SIX Networks GmbH" file="RequestHandlerRequireGameContentDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Mini.Applications
{
    public class RequestHandlerRequireGameContentDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        readonly ISetupGameStuff _setup;
        readonly IRequestHandler<TRequest, TResponse> _target;

        public RequestHandlerRequireGameContentDecorator(IRequestHandler<TRequest, TResponse> target,
            ISetupGameStuff setup) {
            _target = target;
            _setup = setup;
        }

        public TResponse Handle(TRequest request) {
            if (request is INeedGameContents)
                _setup.HandleGameContentsWhenNeeded().Wait();
            return _target.Handle(request);
        }
    }


    public class AsyncRequestHandlerRequireGameContentDecorator<TRequest, TResponse> :
        IAsyncRequestHandler<TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        readonly ISetupGameStuff _setup;
        readonly IAsyncRequestHandler<TRequest, TResponse> _target;

        public AsyncRequestHandlerRequireGameContentDecorator(IAsyncRequestHandler<TRequest, TResponse> target,
            ISetupGameStuff setup) {
            _target = target;
            _setup = setup;
        }

        public async Task<TResponse> HandleAsync(TRequest request) {
            if (request is INeedGameContents)
                await _setup.HandleGameContentsWhenNeeded().ConfigureAwait(false);
            return await _target.HandleAsync(request).ConfigureAwait(false);
        }
    }

    public interface INeedGameContents {}
}