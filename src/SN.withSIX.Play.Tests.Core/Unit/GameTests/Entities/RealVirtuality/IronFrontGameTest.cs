// <copyright company="SIX Networks GmbH" file="IronFrontGameTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Tests.Core.Unit.GameTests.Extensions;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests.Entities.RealVirtuality
{
    [TestFixture, Category("Iron Front")]
    public class IronFrontGameTest
    {
        [SetUp]
        public void SetUp() {
            _game = new IronFrontGame(Guid.NewGuid(), new GameSettingsController());
            _settings = _game.Settings;
            _settings.Directory = @"C:\temp".ToAbsoluteDirectoryPath();
        }

        Arma2FreeSettings _settings;
        IronFrontGame _game;

        [Test]
        public async Task CanLaunch() {
            _game.Settings.LaunchUsingSteam = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGameWithSteamLegacy(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\ironfront.exe");
            parameters.SteamAppId.Should().Be(91330);
        }

        [Test]
        public async Task CanLaunchWithServerMode() {
            _settings.ServerMode = true;
            var mediator = A.Fake<IRealVirtualityLauncher>();

            var parameters = await mediator.FakeLaunchGame(_game);

            parameters.LaunchExecutable.ToString().Should().Be(@"C:\temp\ironfrontserver.exe");
        }

        [Test]
        public async Task CanQueryServers() {
            var mediator = A.Fake<IGameServerQueryHandler>();
            var info = await mediator.FakeGamespyServerQuery(_game).ConfigureAwait(false);

            info.Tag.Should().Be("IFpc");
        }

        [Test]
        public void HasCorrectMetaData() {
            _game.MetaData.Name.Should().Be("Iron Front: Liberation 1944");
            _game.MetaData.Author.Should().Be("Deep Silver");
        }
    }
}