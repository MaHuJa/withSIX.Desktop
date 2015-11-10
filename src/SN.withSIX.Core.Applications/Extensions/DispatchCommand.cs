// <copyright company="SIX Networks GmbH" file="DispatchCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Core.Applications.Extensions
{
    public interface IDispatchCommand
    {
        Task Dispatch(IMediator mediator);
    }

    public class DispatchCommand<T> : IDispatchCommand
        where T : IAsyncRequest<UnitType>
    {
        readonly T _command;

        public DispatchCommand(T command) {
            _command = command;
        }

        public Task Dispatch(IMediator mediator) {
            return mediator.RequestAsync(_command);
        }
    }
}