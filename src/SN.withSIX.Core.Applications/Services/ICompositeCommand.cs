// <copyright company="SIX Networks GmbH" file="ICompositeCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Core.Applications.Services
{
    public interface ICompositeCommand<TResponseData>
    {
        Task<TResponseData> Execute(IMediator mediator);
    }

    public interface ICompositeVoidCommand : ICompositeCommand<UnitType> {}

    public abstract class CompositeCommand<TResponseData> : ICompositeCommand<TResponseData>
    {
        public abstract Task<TResponseData> Execute(IMediator mediator);
    }

    public abstract class CompositeCommandBasic<TResponseData> : CompositeCommand<TResponseData>
    {
        readonly IReadOnlyCollection<IAsyncVoidCommand> _commands;
        readonly IAsyncRequest<TResponseData> _request;

        protected CompositeCommandBasic(IAsyncRequest<TResponseData> request, params IAsyncVoidCommand[] commands) {
            _request = request;
            _commands = commands;
        }

        // Basic because we just execute the commands in order, and then the query
        // if you need more control, e.g handle exceptions at each stage, use the CompositeCommand<TResponseData> instead and implement Execute
        public override async Task<TResponseData> Execute(IMediator mediator) {
            foreach (var c in _commands)
                await mediator.RequestAsync(c).ConfigureAwait(false);
            return await mediator.RequestAsync(_request).ConfigureAwait(false);
        }
    }

    public abstract class CompositeCommandBasicVoid : CompositeCommandBasic<UnitType>
    {
        protected CompositeCommandBasicVoid(IAsyncRequest<UnitType> request, params IAsyncVoidCommand[] commands)
            : base(request, commands) {}
    }
}