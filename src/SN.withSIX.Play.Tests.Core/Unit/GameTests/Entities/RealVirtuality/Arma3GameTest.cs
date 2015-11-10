// <copyright company="SIX Networks GmbH" file="Arma3GameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using ShortBus;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Arma 3")]
    public class Arma3GameTest
    {
        [SetUp]
        public void SetUp() {
            _game = new Arma3Game(Guid.NewGuid(), new GameSettingsController(),
                new Arma3Game.AllInArmaGames(A.Fake<Arma1Game>(), A.Fake<Arma2Game>(), A.Fake<Arma2FreeGame>(),
                    A.Fake<Arma2OaGame>(),
                    A.Fake<TakeOnHelicoptersGame>()));
            _settings = _game.Settings;
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
        }

        Arma3Settings _settings;
        Arma3Game _game;

        [Test]
        public async Task CanLaunch() {
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGameWithSteam(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\arma3.exe");
            parameters.SteamAppId.Should().Be(107410);
        }

        [Test]
        public async Task CanLaunchWithResetGameKeyEachLaunch() {
            _settings.ResetGameKeyEachLaunch = true;
            var parPath = Path.Combine(Path.GetTempPath(), _game.Id + ".txt").ToAbsoluteFilePath();
            var launcher = A.Fake<IRealVirtualityLauncher>();
            var specs = new List<LaunchGameInfoBase>();
            A.CallTo(() => launcher.Launch(A<LaunchGameInfo>._))
                .Invokes((IAsyncRequest<Process> x) => specs.Add((LaunchGameInfo) x))
                .ReturnsLazily(() => Task.FromResult(Process.GetCurrentProcess()));
            A.CallTo(() => launcher.WriteParFile(A<WriteParFileInfo>.That.Matches(x => x.GameId == _game.Id)))
                .ReturnsLazily(() => Task.FromResult(parPath));

            await _game.Launch(GameTest.GameLauncher(_game, launcher)).ConfigureAwait(false);

            specs.Should().HaveCount(2);
            specs[0].StartupParameters.Should().BeEquivalentTo(new[] {"-doNothing"});
            specs[0].Should().BeOfType<LaunchGameWithSteamLegacyInfo>();
            specs[1].StartupParameters.Should()
                .BeEquivalentTo(new[] {"-par=" + parPath, "-nosplash", "-nofilepatching"});
            specs[1].Should().BeOfType<LaunchGameWithSteamInfo>();
        }

        [Test]
        public async Task CanLaunchWithServerMode() {
            _settings.ServerMode = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGameWithSteam(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\arma3server.exe");
        }

        [Test]
        public async Task CanQueryServers() {
            var mediator = A.Fake<IGameServerQueryHandler>();

            await _game.QueryServers(mediator).ConfigureAwait(false);

            A.CallTo(() => mediator.Query(A<GamespyServersQuery>
                .That.Matches(x => x.Tag == "arma3pc")))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => mediator.Query(A<SourceServersQuery>
                .That.Matches(x => x.Tag == "arma3")))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void HasCorrectMetaData() {
            _game.MetaData.Name.Should().Be("Arma 3");
            _game.MetaData.Author.Should().Be("Bohemia Interactive");
        }


        [Test]
        public void SupportsCorrectMissionTypes([Values(GameMissionType.Arma3Mission)] GameMissionType type) {
            _game.SupportsContent(new Mission(Guid.Empty) { ContentType = type }).Should().BeTrue();
        }

        [Test]
        public void SupportsCorrectModTypes(
            [Values(GameModType.Arma3Mod, GameModType.Arma3StMod, GameModType.Rv4Mod, GameModType.Rv4MinMod,
                GameModType.Rv3MinMod, GameModType.Rv2MinMod, GameModType.RvMinMod)] GameModType type) {
                    _game.SupportsContent(new Mod(Guid.Empty) { Type = type }).Should().BeTrue();
        }
    }
}