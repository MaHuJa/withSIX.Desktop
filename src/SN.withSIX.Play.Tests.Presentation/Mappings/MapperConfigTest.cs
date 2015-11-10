// <copyright company="SIX Networks GmbH" file="MapperConfigTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Presentation.Wpf.Services;

namespace SN.withSIX.Play.Tests.Presentation.Mappings
{
    public abstract class GameWithDlc : Game, IHaveDlc
    {
        protected GameWithDlc(Guid id, GameSettings settings) : base(id, settings) {}
        public abstract IEnumerable<Dlc> Dlcs { get; }
    }

    public abstract class FakeDlc : Dlc
    {
        protected FakeDlc(Guid id) : base(id) {}
    }

    [TestFixture]
    public class MapperConfigTest
    {
        static GameWithDlc SetupFakeGame() {
            var id = Guid.NewGuid();
            var game = A.Fake<GameWithDlc>(o => o.WithArgumentsForConstructor(new object[] {id, A.Fake<GameSettings>()}));
            game.CallsTo(x => x.MetaData)
                .Returns(new GameMetaData {
                    Name = "Test Name",
                    Author = "Test Author",
                    Description = "Test Description",
                    Slug = "test-slug",
                    StoreUrl = "http://teststoreurl".ToUri(),
                    SupportUrl = @"http://testsupporturl".ToUri(),
                    ReleasedOn = new DateTime(2008, 1, 2)
                });
            var gameDir = "C:\\temp\\some game".ToAbsoluteDirectoryPath();
            var executable = "C:\\temp\\some game\\game.exe".ToAbsoluteFilePath();
            game.CallsTo(x => x.InstalledState)
                .Returns(new InstalledState(executable, executable, gameDir, gameDir, version: new Version(1, 1, 1)));
            A.CallTo(game).Where(x => x.Method.Name == "GetStartupLine").WithReturnType<string>()
                .Returns("\"C:\\temp\\some game\\game.exe\" -mod= -startupswitch");
            game.CallsTo(x => x.Dlcs)
                .Returns(new[] {SetupFakeDlc()});
            return game;
        }

        static Dlc SetupFakeDlc() {
            var id = Guid.NewGuid();
            var dlc = A.Fake<FakeDlc>(o => o.WithArgumentsForConstructor(new Object[] {id}));
            dlc.CallsTo(x => x.MetaData)
                .Returns(new DlcMetaData {
                    Name = "Test Dlc Name",
                    Author = "Test Dlc Author",
                    Description = "Test Dlc Description"
                });
            return dlc;
        }

        [Test]
        public void DlcDataMapping() {
            var mapper = new GameMapperConfig();
            var dlc = SetupFakeDlc();

            var dataModel = mapper.Map<DlcDataModel>(dlc);

            dataModel.Name.Should().Be("Test Dlc Name");
            dataModel.Author.Should().Be("Test Dlc Author");
            dataModel.Description.Should().Be("Test Dlc Description");
        }

        [Test]
        public void GameDataListMapping() {
            var mapper = new GameMapperConfig();
            var game = SetupFakeGame();
            var data = new List<Game> {game};

            var list = mapper.Map<List<GameDataModel>>(data);

            list.Should().HaveCount(data.Count);
            list[0].Name.Should().Be(data[0].MetaData.Name);
            list[0].StartupLine.Should().Be(data[0].StartupLine);
        }

        [Test]
        public void GameDataMapping() {
            var mapper = new GameMapperConfig();
            var game = SetupFakeGame();

            var dataModel = mapper.Map<GameDataModel>(game);

            dataModel.Id.Should().Be(game.Id);
            dataModel.Author.Should().Be("Test Author");
            dataModel.Name.Should().Be("Test Name");
            dataModel.Description.Should().Be("Test Description");
            dataModel.Slug.Should().Be("test-slug");
            dataModel.StoreUrl.Should().Be("http://teststoreurl");
            dataModel.SupportUrl.Should().Be("http://testsupporturl");
            dataModel.ReleasedOn.Should().Be(new DateTime(2008, 1, 2));

            dataModel.Directory.Should().Be("C:\\temp\\some game");
            dataModel.Executable.Should().Be("C:\\temp\\some game\\game.exe");
            dataModel.StartupLine.Should().Be("\"C:\\temp\\some game\\game.exe\" -mod= -startupswitch");
            dataModel.IsInstalled.Should().BeTrue();
            dataModel.Version.Should().Be(new Version(1, 1, 1));

            dataModel.Dlcs.Should().HaveCount(game.Dlcs.Count());
        }
    }
}