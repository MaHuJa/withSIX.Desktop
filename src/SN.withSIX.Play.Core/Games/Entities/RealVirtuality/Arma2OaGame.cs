// <copyright company="SIX Networks GmbH" file="Arma2OaGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma2OaGame : Arma2Game, IHaveDlc
    {
        static readonly SteamInfo steamInfo = new SteamInfo(33930, "Arma 2 Operation Arrowhead");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Arma 2: Operation Arrowhead",
            ShortName = "ARMA II: OA",
            Author = "Bohemia Interactive",
            Description =
                @"Three years after the conflict in Chernarus, portrayed in the original Arma 2, a new flashpoint in the Green Sea region heats up and coalition forces led by the US Army are sent to Takistan to quickly restore peace and prevent further civilian casualties in this standalone expansion pack to the best military simulator of 2009 – Arma 2.
You will enlist into various roles within the US Army, from basic infantrymen, through special operatives, to pilots and tank crew in this new installment in the award winning line up of military simulators for PC from Bohemia Interactive.",
            Slug = "arma-2",
            StoreUrl = "https://store.bistudio.com/military-simulations-games?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://www.arma2.com/customer-support/support_en.html".ToUri(),
            ReleasedOn = new DateTime(2009, 1, 1)
        };
        static readonly SeparateClientAndServerExecutable executables =
            new SeparateClientAndServerExecutable("arma2oa.exe", "arma2oaserver.exe");
        static readonly RegistryInfo registryInfo = new RegistryInfo(BohemiaStudioRegistry + @"\ArmA 2 OA", "main");
        static readonly RvProfileInfo profileInfo = new RvProfileInfo("Arma 2", "Arma 2 other profiles",
            "ArmA2OaProfile");
        static readonly IEnumerable<Dlc> dlcs = new Dlc[] {
            new ACRDlc(new Guid("bb2f0b97-0fed-4432-b88d-1ca3c6687b32")),
            new BAFDlc(new Guid("b6e8d121-71d0-4de1-a05f-0b963b9c3712")),
            new PMCDlc(new Guid("37a41560-fe6c-4ce8-9d3c-4feafd3a36a0"))
        };
        static readonly IEnumerable<string> defaultModFolders = new[] {"expansion"};
        static readonly IEnumerable<GameModType> supportedModTypes = new[] {
            GameModType.Arma2Mod, GameModType.Arma2OaMod, GameModType.Arma2OaStMod, GameModType.Rv3Mod,
            GameModType.Rv3MinMod, GameModType.Rv2MinMod, GameModType.RvMinMod
        };
        static readonly GamespyServersQuery gamespyServersQuery = new GamespyServersQuery("arma2oapc");
        static readonly SourceServersQuery sourceServersQuery = new SourceServersQuery("arma2arrowpc");
        static readonly ServersQuery[] serverQueries = {sourceServersQuery, gamespyServersQuery};

        public Arma2OaGame(Guid id, GameSettingsController settingsController)
            : this(id, new Arma2OaSettings(id, new ArmaStartupParams(DefaultStartupParameters), settingsController)) {}

        protected Arma2OaGame(Guid id, Arma2OaSettings settings)
            : base(id, settings) {
            Settings = settings;
        }

        public new Arma2OaSettings Settings { get; }
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
            get { return GetServerQueryInfo(); }
        }
        public virtual IEnumerable<Dlc> Dlcs
        {
            get { return dlcs; }
        }

        public override bool SupportsServerType(string type) {
            return serverQueries.Select(x => x.Tag).Contains(type);
        }

        protected override IAbsoluteFilePath GetLaunchExecutable() {
            var executable = base.GetLaunchExecutable();
            var beExecutable = executable.ToString().Replace("arma2oa.exe", "arma2oa_be.exe").ToAbsoluteFilePath();
            return beExecutable.Exists ? beExecutable : executable;
        }

        public override IEnumerable<IAbsolutePath> GetAdditionalLaunchMods() {
            return GetDefaultModFolders().Concat(base.GetAdditionalLaunchMods());
        }

        IEnumerable<IAbsolutePath> GetDefaultModFolders() {
            return defaultModFolders.Select(x => InstalledState.Directory.GetChildDirectoryWithName(x));
        }

        protected override IEnumerable<GameModType> GetSupportedModTypes() {
            return supportedModTypes;
        }

        ServersQuery GetServerQueryInfo() {
            switch (Settings.ServerQueryMode) {
            case ServerQueryMode.All:
                return gamespyServersQuery;
            // TODO: This should rather be based on the query type of the server..
            case ServerQueryMode.Gamespy: {
                return gamespyServersQuery;
            }
            case ServerQueryMode.Steam: {
                return sourceServersQuery;
            }
            default: {
                throw new InvalidOperationException("Unknown server query: " + Settings.ServerQueryMode);
            }
            }
        }

        public override Task<IEnumerable<ServerQueryResult>> QueryServers(
            IGameServerQueryHandler queryHandler) {
            switch (Settings.ServerQueryMode) {
            case ServerQueryMode.All:
                return serverQueries.ServersQuery(queryHandler);
            case ServerQueryMode.Gamespy:
                return queryHandler.Query(gamespyServersQuery);
            case ServerQueryMode.Steam:
                return queryHandler.Query(sourceServersQuery);
            default:
                throw new InvalidOperationException("Unknown server query: " + Settings.ServerQueryMode);
            }
        }

        public override Task QueryServer(ServerQueryState state) {
            switch (Settings.ServerQueryMode) {
            case ServerQueryMode.All: {
                if (state.Server.QueryMode == ServerQueryMode.Steam)
                    return sourceServersQuery.QueryServer(state);
                if (state.Server.QueryMode == ServerQueryMode.Gamespy)
                    return gamespyServersQuery.QueryServer(state);
                return Task.WhenAll(sourceServersQuery.QueryServer(state), gamespyServersQuery.QueryServer(state));
            }
            case ServerQueryMode.Gamespy:
                return gamespyServersQuery.QueryServer(state);
            case ServerQueryMode.Steam:
                return sourceServersQuery.QueryServer(state);
            default:
                throw new InvalidOperationException("Unknown server query: " + Settings.ServerQueryMode);
            }
        }

        protected override Tuple<string[], string[]> StartupParameters() {
            var startupBuilder = new StartupBuilder(this, new Arma2OaModListBuilder());
            return startupBuilder.GetStartupParameters(GetStartupSpec());
        }

        protected override async Task<IEnumerable<string>> BuildStartupParameters(IRealVirtualityLauncher launcher) {
            var startupParams = await base.BuildStartupParameters(launcher).ConfigureAwait(false);
            var installedState = InstalledState;
            if (installedState.IsInstalled && installedState.LaunchExecutable.FileName == "arma2oa_be.exe")
                return new[] {"0", "0"}.Concat(startupParams);
            return startupParams;
        }

        class ACRDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "ACR",
                Author = "Bohemia Interactive",
                Description =
                    @"Civil war in Bystrica is at its end. But it’s not the end of fear for the people. War criminals like Colonel Miyovic still terrorize the country with their militia looting and murdering civilians. Forces of the Czech Republic Army are sent to restore order.
After restoring some order in Takistan; Allied forces were able to send in a small number of provisional reconstruction teams. Unfortunately local insurgents are still planning operations to damage the reconstruction process. However Czech forces are operating in the area and that should help change the status quo."
            };
            public ACRDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }

        protected class Arma2OaModListBuilder : ModListBuilder
        {
            static readonly string[] ifModFolders = {"@LIB_DLC_1", "@IF_Other_Addons", "@IF"};
            static readonly string[] ifMainModFolders = {"@IF"};
            static readonly string[] ifModFoldersLite = ifModFolders.Select(x => x + "_Lite").ToArray();
            static readonly string[] ifMainModFoldersLite = ifMainModFolders.Select(x => x + "_Lite").ToArray();

            protected override void ProcessMods() {
                ProcessIronFrontMods();
                base.ProcessMods();
            }

            void ProcessIronFrontMods() {
                if (!InputMods.Any(x => ifMainModFolders.ContainsIgnoreCase(x.Name)))
                    return;
                InputMods.RemoveAll(IsIronFrontFullOrLiteMod);
                OutputMods.AddRange(ExistingMods(GetOaPaths().Where(x => x != null).ToArray(), ifModFolders));
            }

            IEnumerable<IAbsoluteDirectoryPath> GetOaPaths() {
                return new[] {Spec.ModPath, Spec.GamePath};
            }

            static bool IsIronFrontFullOrLiteMod(IMod x) {
                return ifMainModFolders.ContainsIgnoreCase(x.Name) || ifMainModFoldersLite.ContainsIgnoreCase(x.Name) ||
                       ifModFolders.ContainsIgnoreCase(x.Name)
                       || ifModFoldersLite.ContainsIgnoreCase(x.Name);
            }

            static IEnumerable<IAbsoluteDirectoryPath> ExistingMods(IAbsoluteDirectoryPath[] paths, params string[] mods) {
                return paths.Any()
                    ? mods.Select(
                        x => paths.Select(path => path.GetChildDirectoryWithName(x)).FirstOrDefault(p => p.Exists))
                        .Where(x => x != null)
                    : Enumerable.Empty<IAbsoluteDirectoryPath>();
            }
        }

        class BAFDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "BAF",
                Author = "Bohemia Interactive",
                Description =
                    "Two months after the Allied military victory in Takistan, the new government is restoring the war-torn country. NATO forces assisting in this effort face the threat of insurgency, waged by the remnants of the defeated Takistani army in the mountaineous regions of Takistan. Company team of British paratroopers patrolling the treacherous mountains in Zargabad's vicinity is ordered to battle the amassing guerilla warriors."
            };
            public BAFDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }

        class PMCDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "PMC",
                Author = "Bohemia Interactive",
                Description =
                    "One year after British and coalition armed forces successfully quelled the insurgent uprising in Takistan, the NATO Green Sea deployment is in the process of a strategic drawdown of combat troops in the region. Private military contractors shoulder the burden of the increased workload, with competition rising between the multinational organisations for lucrative security contracts. Private Military Company, ION, Inc. (formerly Black Element), successfully bid for a contract - codenamed Black Gauntlet - to provide security for a UN investigation team as they seek to piece together information regarding Takistan's abandoned nuclear weapons programme."
            };
            public PMCDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }
    }

    public interface IHaveDlc
    {
        IEnumerable<Dlc> Dlcs { get; }
    }
}