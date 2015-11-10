// <copyright company="SIX Networks GmbH" file="LaunchGameCommandHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Games.Services.Infrastructure;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Application.Usecases
{
    public class LaunchGameCommandHandler : IAsyncRequestHandler<LaunchGameCommand, int>
    {
        readonly IGameContext _context;
        readonly IGameLauncherFactory _factory;

        public LaunchGameCommandHandler(IGameLauncherFactory factory, IGameContext context) {
            _factory = factory;
            _context = context;
        }

        public async Task<int> HandleAsync(LaunchGameCommand command) {
            try {
                var game = _context.Games.Find(command.Id);
                return await game.Launch(_factory).ConfigureAwait(false);
            } catch (Exception e) {
                // OperationCanceledException if prechecks cancelled
                throw new LaunchGameCommandException(e.Message, e);
            }
        }

        class LaunchGameCommandException : Exception
        {
            public LaunchGameCommandException(string message, Exception exception) : base(message, exception) {}
        }
    }

    public class LaunchGameCommand : IAsyncRequest<int>
    {
        public Guid Id { get; set; }
    }
}