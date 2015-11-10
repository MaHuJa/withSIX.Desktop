// <copyright company="SIX Networks GmbH" file="Arma1Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma1Game : ArmaGame
    {
        static readonly SteamInfo steamInfo = new SteamInfo(2780, "ArmA Armed Assault");
        static readonly GamespyServersQuery serverQueryInfo = new GamespyServersQuery("armapc");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "ARMA: Armed Assault",
            ShortName = "ARMA I",
            Author = "Bohemia Interactive",
            Description =
                @"ArmA is a first person tactical military shooter with large elements of realism and simulation. This game features a blend of large-scale military conflict spread over large areas alongside the more closed quarters battle. The player will find himself thrust in the midst of an engaging and expanding storyline, fighting against smart, aggressive enemies who will continually provide a challenge over a massive landscape.",
            StoreUrl = "https://store.bistudio.com/military-simulations-games?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://www.arma2.com/customer-support/support_en.html".ToUri(),
            ReleasedOn = new DateTime(2005, 1, 1),
            Slug = "arma"
        };
        static readonly SeparateClientAndServerExecutable executables =
            new SeparateClientAndServerExecutable("arma.exe", "armaserver.exe");
        static readonly RegistryInfo registryInfo = new RegistryInfo(BohemiaStudioRegistry + @"\ArmA", "main");
        static readonly RvProfileInfo profileInfo = new RvProfileInfo("Arma", "Arma other profiles", "ArmaProfile");
        static readonly IEnumerable<string> defaultModFolders = new[] {"dbe1"};
        static readonly IEnumerable<GameModType> supportedModTypes = new[]
        {GameModType.Arma1Mod, GameModType.Arma1StMod, GameModType.Rv2Mod, GameModType.Rv2MinMod, GameModType.RvMinMod};
        public Arma1Game(Guid id, GameSettingsController settingsController) : base(id, settingsController) {}
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

        public override IEnumerable<IAbsolutePath> GetAdditionalLaunchMods() {
            return GetDefaultModFolders().Concat(base.GetAdditionalLaunchMods());
        }

        IEnumerable<IAbsoluteDirectoryPath> GetDefaultModFolders() {
            return defaultModFolders.Select(x => InstalledState.Directory.GetChildDirectoryWithName(x));
        }

        protected override IEnumerable<GameModType> GetSupportedModTypes() {
            return supportedModTypes;
        }

        public override bool SupportsContent(Mission mission) {
            return false;
        }

        public override Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler) {
            return queryHandler.Query(serverQueryInfo);
        }

        public override Task QueryServer(ServerQueryState state) {
            return serverQueryInfo.QueryServer(state);
        }
    }
}