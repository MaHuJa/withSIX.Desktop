// <copyright company="SIX Networks GmbH" file="IronFrontGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class IronFrontGame : Arma2FreeGame, IHaveDlc
    {
        static readonly SteamInfo steamInfo = new SteamInfo(91330, "IronFront");
        static readonly GamespyServersQuery serverQueryInfo = new GamespyServersQuery("IFpc");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Iron Front: Liberation 1944",
            ShortName = "Iron Front",
            Author = "Deep Silver",
            Description =
                @"Experience the deciding battles of the summer offensive of 1944 as either the Germans or the Red Army. Starting out as a simple soldier on either side, you will experience nerve-wracking, tactical combat – always caught between your orders from your senior, and reality on the battlefield. In the course of the story, both protagonists will be promoted to command tanks and even fighter planes in addition to their own troops. Southern Poland, where the hostile armies will clash with the latest weapons, is the setting. The Germans’ objective is clear – consolidate and regroup the remaining troops, stop the Red Army’s advance at all cost, or at least slow it down. As a Red Army soldier, the task less complex, but no less challenging: Break the resistance to enable an attack on Germany.",
            Slug = "iron-front",
            StoreUrl = "http://store.steampowered.com/app/91330/".ToUri(),
            SupportUrl = @"http://ironfront.deepsilver.com".ToUri(),
            ReleasedOn = new DateTime(2008, 1, 1)
        };
        static readonly SeparateClientAndServerExecutable executables =
            new SeparateClientAndServerExecutable("ironfront.exe", "ironfrontserver.exe");
        static readonly RegistryInfo registryInfo = new RegistryInfo(BohemiaStudioRegistry + @"\Ironfront", "main");
        static readonly RvProfileInfo profileInfo = new RvProfileInfo("ironfront", "ironfront other profiles",
            "IFProfile");
        static readonly IEnumerable<Dlc> dlcs = new Dlc[] {
            new DDayDlc(new Guid("6cfdc55a-a4de-4ba7-b709-fb6da2885c83"))
        };
        static readonly IEnumerable<GameMissionType> supportedMissionTypes = new[] {GameMissionType.IronFrontMission};

        public IronFrontGame(Guid id, GameSettingsController settingsController)
            : this(id, new Arma2FreeSettings(id, new ArmaStartupParams(DefaultStartupParameters), settingsController)) {}

        IronFrontGame(Guid id, Arma2FreeSettings settings)
            : base(id, settings) {
            Settings = settings;
        }

        protected override SeparateClientAndServerExecutable Executables
        {
            get { return executables; }
        }
        public new Arma2FreeSettings Settings { get; private set; }
        protected override string MissionExtension
        {
            get { return ".ifa"; }
        }
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
        protected override RvProfileInfo ProfileInfo
        {
            get { return profileInfo; }
        }
        public virtual IEnumerable<Dlc> Dlcs
        {
            get { return dlcs; }
        }

        public override bool SupportsContent(Mission mission) {
            return supportedMissionTypes.Contains(mission.ContentType);
        }

        public override Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler) {
            return queryHandler.Query(serverQueryInfo);
        }

        class DDayDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "DLC_1",
                FullName = "DDay",
                Author = "Deepsilver",
                Description = @"Summer 1944. Europe is at war. The Allies are about to attack...
'D-Day' adds the famous landing on the coast of Normandy by the Allied Forces to the tactical World War II shooter 'Iron Front – Liberation 1944'. Merciless gun battles for each centimeter of the beaches await brave soldiers on the side of the Allies and the Wehrmacht alike in this extensive expansion. Brutal combat unfolds on the storm-lashed shores of France, as the future of the world is about to be decided at the Atlantic Wall... and you are in the thick of it! Fight with the Allied Forces against the dogged Wehrmacht defenders, or join the battle on the side of Germany and use any means to stop the Allies' advance."
            };
            public DDayDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }
    }
}