// <copyright company="SIX Networks GmbH" file="DayZGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class DayZGame : RealVirtualityGame, ISupportServers
    {
        const string Name = "DayZ";
        static readonly SourceServersQuery serverQueryInfo = new SourceServersQuery("dayz");
        static readonly SteamInfo steamInfo = new SteamInfo(107410, Name) {DRM = true};
        static readonly GameMetaData metaData = new GameMetaData {
            Name = Name,
            ShortName = Name,
            Author = "Bohemia Interactive",
            StoreUrl = "https://store.bistudio.com/dayz?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://forums.dayzgame.com".ToUri(),
            ReleasedOn = new DateTime(2013, 11, 14)
        };
        static readonly RvProfileInfo profileInfo = new RvProfileInfo(Name, "DayZ - other profiles", "DayZProfile");

        public DayZGame(Guid id, GameSettingsController settingsController)
            : this(id, new DayZSettings(id, new DayZStartupParams(DefaultStartupParameters), settingsController)) {}

        protected DayZGame(Guid id, DayZSettings settings) : base(id, settings) {
            Settings = settings;
        }

        public new DayZSettings Settings { get; }
        protected override SteamInfo SteamInfo
        {
            get { return steamInfo; }
        }
        public override GameMetaData MetaData
        {
            get { return metaData; }
        }
        protected override RvProfileInfo ProfileInfo
        {
            get { return profileInfo; }
        }
        public bool IsServer
        {
            get { return false; }
        }

        public virtual Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler) {
            return queryHandler.Query(serverQueryInfo);
        }

        public Task QueryServer(ServerQueryState state) {
            return serverQueryInfo.QueryServer(state);
        }

        public IFilter GetServerFilter() {
            return Settings.Filter;
        }

        public virtual bool SupportsServerType(string type) {
            return serverQueryInfo.Tag == type;
        }

        public Server CreateServer(ServerAddress address) {
            return new DayzServer(this, address);
        }

        protected override IAbsoluteFilePath GetExecutable() {
            return GetGameDirectory().GetChildFileWithName("dayz.exe");
        }
    }
}