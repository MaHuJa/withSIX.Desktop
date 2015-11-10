// <copyright company="SIX Networks GmbH" file="TakeOnHelicoptersGame.cs">
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
    public class TakeOnHelicoptersGame : ArmaGame, IHaveDlc
    {
        static readonly SteamInfo steamInfo = new SteamInfo(65730, "Take on Helicopters");
        static readonly GamespyServersQuery queryHandler = new GamespyServersQuery("takoncopterpc");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Take on Helicopters",
            ShortName = "TKOH",
            Author = "Bohemia Interactive",
            Description =
                @"Take On Helicopters is the brand new helicopter game from leading independent developer Bohemia Interactive.
Set against the beautiful backdrop of Seattle, Take On Helicopters lets you enjoy the thrilling experience of flying a helicopter. Become a pilot and have fun with the story-driven career mode, time-trials, challenges and multiplayer!",
            Slug = "take-on-helicopters",
            StoreUrl = "https://store.bistudio.com/helicopter-games?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://takeonthegame.com/support".ToUri(),
            ReleasedOn = new DateTime(2010, 1, 1)
        };
        static readonly SeparateClientAndServerExecutable executables =
            new SeparateClientAndServerExecutable("takeonh.exe", "takeonhserver.exe");
        static readonly RegistryInfo registryInfo = new RegistryInfo(BohemiaStudioRegistry + @"\Take on Helicopters",
            "main");
        static readonly RvProfileInfo profileInfo = new RvProfileInfo("Take on Helicopters",
            "Take on Helicopters other profiles", "TakeOnHProfile");
        static readonly IEnumerable<Dlc> dlcs = new Dlc[] {
            new HindsDLc(new Guid("de1700d9-88f4-4233-9836-5e20adb6c645"))
        };
        static readonly IEnumerable<GameModType> supportedModTypes = new[]
        {GameModType.TakeonhMod, GameModType.TakeonhStMod};
        static readonly IEnumerable<GameMissionType> supportedMissionTypes = new[]
        {GameMissionType.TakeOnHelicoptersMission};

        public TakeOnHelicoptersGame(Guid id, GameSettingsController settingsController)
            : base(id, settingsController) {}

        protected override RegistryInfo RegistryInfo
        {
            get { return registryInfo; }
        }
        protected override SeparateClientAndServerExecutable Executables
        {
            get { return executables; }
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
        protected override ServersQuery ServerQueryInfo
        {
            get { return queryHandler; }
        }
        public virtual IEnumerable<Dlc> Dlcs
        {
            get { return dlcs; }
        }

        public override bool SupportsContent(Mission mission) {
            return supportedMissionTypes.Contains(mission.ContentType);
        }

        protected override IEnumerable<GameModType> GetSupportedModTypes() {
            return supportedModTypes;
        }

        public override Task<IEnumerable<ServerQueryResult>> QueryServers(IGameServerQueryHandler queryHandler) {
            return queryHandler.Query(TakeOnHelicoptersGame.queryHandler);
        }

        public override Task QueryServer(ServerQueryState state) {
            return queryHandler.QueryServer(state);
        }

        class HindsDLc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "Hinds",
                Author = "Bohemia Interactive",
                Description =
                    @"Take On Helicopters: Hinds is the first official Downloadable Content (DLC) pack for Take On Helicopters, the brand new helicopter game from Bohemia Interactive. Shifting focus to a more combat-oriented experience, Take On Helicopters: Hinds puts players in the seat of an authentic recreation of the Hind gunship."
            };
            public HindsDLc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }
    }
}