// <copyright company="SIX Networks GmbH" file="TakeOnHelicoptersGameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Take On Helicopters")]
    public class TakeOnHelicoptersGameTest
    {
        [SetUp]
        public void SetUp() {
            _game = new TakeOnHelicoptersGame(Guid.NewGuid(), new GameSettingsController());
            _settings = _game.Settings;
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
        }

        ArmaSettings _settings;
        TakeOnHelicoptersGame _game;

        [Test]
        public async Task CanLaunch() {
            _game.Settings.LaunchUsingSteam = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGameWithSteamLegacy(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\takeonh.exe");
            parameters.SteamAppId.Should().Be(65730);
        }

        [Test]
        public async Task CanLaunchWithServerMode() {
            _settings.ServerMode = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\takeonhserver.exe");
        }

        [Test]
        public async Task CanQueryServers() {
            var mediator = A.Fake<IGameServerQueryHandler>();
            var info = await mediator.FakeGamespyServerQuery(_game).ConfigureAwait(false);

            info.Tag.Should().Be("takoncopterpc");
        }

        [Test]
        public void HasCorrectMetaData() {
            _game.MetaData.Name.Should().Be("Take on Helicopters");
            _game.MetaData.Author.Should().Be("Bohemia Interactive");
        }

        [Test]
        public void SupportsCorrectMissionTypes([Values(GameMissionType.TakeOnHelicoptersMission)] GameMissionType type) {
            _game.SupportsContent(new Mission(Guid.Empty) { ContentType = type }).Should().BeTrue();
        }

        [Test]
        public void SupportsCorrectModTypes(
            [Values(GameModType.TakeonhMod, GameModType.TakeonhStMod)] GameModType type) {
                _game.SupportsContent(new Mod(Guid.Empty) { Type = type }).Should().BeTrue();
        }
    }
}