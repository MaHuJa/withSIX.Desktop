// <copyright company="SIX Networks GmbH" file="Request.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ShortBus;

namespace SN.withSIX.Core.Applications.Services
{
    public interface IQuery<T> : IRead, IRequest<T> {}

    public interface IRead {}

    public interface ICommand<T> : IWrite, IRequest<T> {}

    public interface IWrite {}

    public interface IVoidCommand : ICommand<UnitType> {}

    public interface IVoidCommandHandler<in TCommand> : IRequestHandler<TCommand, UnitType>
        where TCommand : IRequest<UnitType> {}

    public interface IAsyncQuery<T> : IRead, IAsyncRequest<T> {}

    public interface IAsyncCommand<T> : IWrite, IAsyncRequest<T> {}

    public interface IAsyncVoidCommand : IAsyncCommand<UnitType> {}

    public interface IAsyncVoidCommandHandler<in TCommand> : IAsyncRequestHandler<TCommand, UnitType>
        where TCommand : IAsyncRequest<UnitType> {}
}