// <copyright company="SIX Networks GmbH" file="Arma2Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma2Game : ArmaGame
    {
        static readonly SteamInfo steamInfo = new SteamInfo(33910, "Arma 2");
        static readonly GamespyServersQuery serverQueryInfo = new GamespyServersQuery("arma2pc");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Arma 2: Original",
            ShortName = "ARMA II",
            Author = "Bohemia Interactive",
            Description =
                @"In 2009 the fictional post-soviet country of Chernarus is ravaged by civil war. For two crippling years, conflict has raged...
Players explore this volatile world with Force Recon Razor Team, part of a US Marine Corps Expeditionary Unit. The 27th MEU are deployed to Northern Chernarus on a peacekeeping mission with a mandate to prevent further civilian casualties and promote political stability.
ARMA 2 is based on the latest generation technology. It offers a uniquely vast game world, authentic and extremely detailed modern units, weapons, vehicles and environments. The branching, player-driven campaign can be played solo, or cooperatively and is accompanied by a huge range of single and multiplayer game modes.",
            Slug = "arma-2",
            StoreUrl = "https://store.bistudio.com/military-simulations-games?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://www.arma2.com/customer-support/support_en.html".ToUri(),
            ReleasedOn = new DateTime(2008, 1, 2)
        };
        static readonly SeparateClientAndServerExecutable executables =
            new SeparateClientAndServerExecutable("arma2.exe", "arma2server.exe");
        static readonly RegistryInfo registryInfo = new RegistryInfo(BohemiaStudioRegistry + @"\ArmA 2", "main");
        static readonly RvProfileInfo profileInfo = new RvProfileInfo("Arma 2", "Arma 2 other profiles", "Arma2Profile");
        static readonly IEnumerable<GameModType> supportedModTypes = new[] {
            GameModType.Arma2Mod, GameModType.Arma2StMod, GameModType.Arma2CaMod, GameModType.Rv3Mod,
            GameModType.Rv3MinMod, GameModType.Rv2MinMod, GameModType.RvMinMod
        };
        static readonly IEnumerable<GameMissionType> supportedMissionTypes = new[] {GameMissionType.Arma2Mission};

        public Arma2Game(Guid id, GameSettingsController settingsController)
            : base(id, settingsController) {}

        protected Arma2Game(Guid id, ArmaSettings settings) : base(id, settings) {}
        protected override RegistryInfo RegistryInfo
        {
            get { return registryInfo; }
        }
        protected override SteamInfo SteamInfo
        {
            get { return steamInfo; }
        }
        public override GameMetaData MetaData
        {
            get { return metaData; }
        }
        protected override SeparateClientAndServerExecutable Executables
        {
            get { return executables; }
        }
        protected override RvProfileInfo ProfileInfo
        {
            get { return profileInfo; }
        }
        protected override ServersQuery ServerQueryInfo
        {
            get { return serverQueryInfo; }
        }

        public override bool SupportsContent(Mission mission) {
            return supportedMissionTypes.Contains(mission.ContentType);
        }

        protected override IEnumerable<GameModType> GetSupportedModTypes() {
            return supportedModTypes;
        }

        public override Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler) {
            return queryHandler.Query(serverQueryInfo);
        }

        public override Task QueryServer(ServerQueryState state) {
            return serverQueryInfo.QueryServer(state);
        }
    }
}