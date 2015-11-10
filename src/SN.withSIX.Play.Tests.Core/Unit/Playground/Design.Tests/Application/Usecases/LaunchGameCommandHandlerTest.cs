// <copyright company="SIX Networks GmbH" file="LaunchGameCommandHandlerTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using NUnit.Framework;
using ReactiveUI;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Games.Services.Infrastructure;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Application.Usecases;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Tests.Application.Usecases
{
    [TestFixture]
    public class LaunchGameCommandHandlerTest
    {
        [Test]
        public async Task CanLaunch() {
            var mediator = A.Fake<IGameLauncherFactory>();
            var dataContext = A.Fake<IGameContext>();
            var launchGame = new LaunchGameCommandHandler(mediator, dataContext);
            var guid = Guid.NewGuid();
            var fakeGame =
                A.Fake<Game>(options => options.WithArgumentsForConstructor(new Object[] {guid, A.Fake<GameSettings>()}));
            dataContext.CallsTo(x => x.Games)
                .Returns(new InMemoryDbSet<Game, Guid>(new ReactiveList<Game>{fakeGame}));

            await launchGame.HandleAsync(new LaunchGameCommand {Id = guid}).ConfigureAwait(false);

            A.CallTo(() => fakeGame.Launch(mediator))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}