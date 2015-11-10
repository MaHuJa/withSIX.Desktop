// <copyright company="SIX Networks GmbH" file="Arma3Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities.Requirements;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Core.Games.Services;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.RealVirtuality
{
    public class Arma3Game : ArmaGame, IHaveDlc
    {
        const string BattleEyeExe = "arma3battleye.exe";
        static readonly SteamInfo steamInfo = new SteamInfo(107410, "Arma 3") {DRM = true};
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Arma 3",
            ShortName = "ARMA III",
            Author = "Bohemia Interactive",
            Description =
                @"The latest installment in the ARMA universe from leading independent developer Bohemia Interactive.",
            Slug = "arma-3",
            StoreUrl = "https://store.bistudio.com/military-simulations-games?banner_tag=SIXNetworks".ToUri(),
            SupportUrl = @"http://dev.arma3.com".ToUri(),
            ReleasedOn = new DateTime(2013, 9, 10)
        };
        static readonly SeparateClientAndServerExecutable executables =
            new SeparateClientAndServerExecutable("arma3.exe", "arma3server.exe");
        static readonly RegistryInfo registryInfo = new RegistryInfo(BohemiaRegistry + @"\ArmA 3", "main");
        static readonly RvProfileInfo profileInfo = new RvProfileInfo("Arma 3", "Arma 3 - other profiles",
            "Arma3Profile");
        static readonly IEnumerable<Requirement> requirements = new[] {new DirectXRequirement("10.0".ToVersion())};
        static readonly IEnumerable<GameModType> supportedModTypes = new[] {
            GameModType.Arma3Mod, GameModType.Arma3StMod, GameModType.Rv4Mod, GameModType.Rv4MinMod,
            GameModType.Rv3MinMod, GameModType.Rv2MinMod, GameModType.RvMinMod
        };
        static readonly IEnumerable<GameMissionType> supportedMissionTypes = new[] {GameMissionType.Arma3Mission};
        public static readonly string[] Arma2TerrainPacks = {
            "@a3mp", "@AllInArmaTerrainPack",
            "@AllInArmaTerrainPackLite"
        };
        static readonly Dlc[] dlcs = {
            new KartsDlc(new Guid("923d2c03-54a6-498c-8469-d541465c42ae")),
            new HelicoptersDlc(new Guid("f69847a7-5c2d-4051-844f-b306280583ed")),
            new MarksmenDlc(new Guid("35fe4a2c-92f0-44f5-be31-d4aa5c4d226d"))
        };
        static readonly GamespyServersQuery gamespyServersQuery = new GamespyServersQuery("arma3pc");
        static readonly SourceServersQuery sourceServersQuery = new SourceServersQuery("arma3");
        static readonly ServersQuery[] serverQueries = {sourceServersQuery, gamespyServersQuery};
        readonly string[] _a3MpCategories = {"Island", "Objects (Buildings, Foliage, Trees etc)"};
        readonly AllInArmaGames _aiaGames;

        public Arma3Game(Guid id, GameSettingsController settingsController, AllInArmaGames aiaGames)
            : this(
                id, new Arma3Settings(id, new Arma3StartupParameters(DefaultStartupParameters), settingsController),
                aiaGames) {}

        Arma3Game(Guid id, Arma3Settings settings, AllInArmaGames aiaGames) : base(id, settings) {
            _aiaGames = aiaGames;
            Settings = settings;
        }

        public new Arma3Settings Settings { get; }
        protected override IEnumerable<Requirement> Requirements
        {
            get { return requirements; }
        }
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

        public IEnumerable<LocalModsContainer> GetSubGamesLocalMods() {
            return _aiaGames.GetLocalMods().Reverse();
        }

        public override bool SupportsContent(Mission mission) {
            return supportedMissionTypes.Contains(mission.ContentType);
        }

        public override bool SupportsContent(IMod mod) {
            return GetSupportedModTypes().Contains(mod.Type)
                   || (CalculatedSettings.HasArma2CompatibilityPack && Arma2COGame.SupportedModTypes.Contains(mod.Type))
                   ||
                   (CalculatedSettings.HasArma2TerrainPacks &&
                    mod.Categories.Any(x => _a3MpCategories.ContainsIgnoreCase(x)))
                   || (CalculatedSettings.HasAllInArmaLegacy && _aiaGames.SupportsContent(mod));
        }

        protected override IEnumerable<GameModType> GetSupportedModTypes() {
            return supportedModTypes;
        }

        IEnumerable<GameController> GetControllers() {
            var controllers = new[] {Controller};
            return !CalculatedSettings.HasAllInArmaLegacy
                ? controllers
                : _aiaGames.GetControllers().Concat(controllers).ToArray();
        }

        public override async Task<Exception[]> UpdateSynq(IContentManager modList, bool isInternetAvailable) {
            if (!InstalledState.IsInstalled || !ModPaths.IsValid)
                return new Exception[0];

            var controllers = GetControllers().Where(x => x.Game.InstalledState.IsInstalled)
                .ToArray();

            foreach (var controller in controllers)
                await controller.UpdateBundleManager().ConfigureAwait(false);
            var ex = new List<Exception>();
            if (isInternetAvailable) {
                foreach (var bm in controllers.Select(x => x.BundleManager).Distinct())
                    ex.Add(await UpdateRemotes(bm).ConfigureAwait(false));
            }

            foreach (var controller in controllers)
                controller.Update();

            if (InstalledState.IsInstalled) {
                ProcessSynqMods(this, modList, controllers);
                ProcessSynqMissions(this, modList, Controller);
            }

            return ex.Where(x => x != null).ToArray();
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

        protected override Task<Process> PerformNewLaunch(IRealVirtualityLauncher launcher) {
            if (!Settings.ServerMode)
                return LaunchSteamModern(launcher);

            return LaunchNormal(launcher);
        }

        protected override IAbsoluteFilePath GetLaunchExecutable() {
            return Settings.ServerMode || !GetBattleEyeClientExectuable().Exists
                ? base.GetLaunchExecutable()
                : GetBattleEyeClientExectuable();
        }

        protected IAbsoluteFilePath GetBattleEyeClientExectuable() {
            return GetExecutable().GetBrotherFileWithName(BattleEyeExe);
        }

        protected override async Task<IEnumerable<string>> BuildStartupParameters(IRealVirtualityLauncher launcher) {
            // TODO: Add the BE params in the shared Startupparameter methods instead
            var defParams = await base.BuildStartupParameters(launcher).ConfigureAwait(false);
            return Settings.ServerMode || !GetBattleEyeClientExectuable().Exists
                ? defParams
                : AddBattleEyeLaunchParameters(defParams);
        }

        protected override async Task<IReadOnlyCollection<string>> BuildStartupParametersForShortcut(
            IRealVirtualityLauncher launcher,
            string identifier) {
            // TODO: Add the BE params in the shared Startupparameter methods instead
            var defParams = await base.BuildStartupParametersForShortcut(launcher, identifier).ConfigureAwait(false);
            return Settings.ServerMode || !GetBattleEyeClientExectuable().Exists
                ? defParams
                : AddBattleEyeLaunchParameters(defParams);
        }

        static List<string> AddBattleEyeLaunchParameters(IEnumerable<string> defParams) {
            var sParams = defParams.ToList();
            sParams.Insert(0, "2");
            sParams.Insert(1, "1");
            return sParams;
        }

        public override void UpdateModStates(IReadOnlyCollection<IMod> mods) {
            foreach (var m in mods)
                m.Controller.UpdateState(GetSupportedGame(m));
        }

        ISupportModding GetSupportedGame(IMod mod) {
            if (!CalculatedSettings.HasAllInArmaLegacy)
                return this;
            return _aiaGames.GetSupportedGame(mod) ?? this;
        }

        protected override Tuple<string[], string[]> StartupParameters() {
            var startupBuilder = new StartupBuilder(this, new Arma3ModListBuilder(_aiaGames));
            return startupBuilder.GetStartupParameters(GetStartupSpec());
        }

        // TODO: Proper detection for ARMA3 DLCs; doesnt seem to work the same as before..
        // Steam AppID
        class KartsDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "Karts",
                Author = "Bohemia Interactive",
                Description =
                    @"Start your engines - this is Arma 3 Karts! When we unveiled our 2014 April Fools joke via the Splendid Split parody video, some of you were quite keen for us to release the go-karts package for real. And here it is, for everyone to try. Enjoy this little sidestep in our typically military sandbox!"
            };
            public KartsDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }

        class HelicoptersDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "Helicopters",
                Author = "Bohemia Interactive",
                Description =
                    @"Master the exhilarating experience of rotary flight in Arma 3 Helicopters. Together with the optional RotorLib Flight Dynamics Model (FDM), Arma 3’s Helicopters DLC will deliver two brand new transport helicopters, new playable content, new Steam Achievements, and more."
            };
            public HelicoptersDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }

        class MarksmenDlc : Dlc
        {
            static readonly DlcMetaData metaData = new DlcMetaData {
                Name = "Marksmen",
                Author = "Bohemia Interactive",
                Description =
                    @"Hone your shooting skills and engage in long-distance combat. The Arma 3 Marksmen DLC will include new weapons, playable content, Steam Achievements, and more."
            };
            public MarksmenDlc(Guid id) : base(id) {}
            public override DlcMetaData MetaData
            {
                get { return metaData; }
            }
        }

        public class AllInArmaGames
        {
            readonly Game[] _games;
            readonly ISupportModding[] _gamesSupportModding;
            internal readonly Arma1Game Arma1;
            internal readonly Arma2Game Arma2;
            internal readonly Arma2FreeGame Arma2Free;
            internal readonly Arma2OaGame Arma2Oa;
            internal readonly TakeOnHelicoptersGame TakeOn;

            public AllInArmaGames(Arma1Game arma1Game, Arma2Game arma2Game, Arma2FreeGame arma2FreeGame,
                Arma2OaGame arma2OaGame,
                TakeOnHelicoptersGame takeOnHelicoptersGame) {
                Arma1 = arma1Game;
                Arma2 = arma2Game;
                Arma2Free = arma2FreeGame;
                Arma2Oa = arma2OaGame;
                TakeOn = takeOnHelicoptersGame;
                _games = new Game[] {arma1Game, arma2FreeGame, arma2Game, arma2OaGame, takeOnHelicoptersGame};
                _gamesSupportModding = _games.OfType<ISupportModding>().ToArray();
            }

            internal Game Find(Guid id) {
                return _games.First(x => x.Id == id);
            }

            internal bool SupportsContent(IMod mod) {
                return GetInstalledGames().Any(x => x.SupportsContent(mod));
            }

            IEnumerable<ISupportModding> GetInstalledGames() {
                return _gamesSupportModding.Where(x => x.InstalledState.IsInstalled);
            }

            internal IEnumerable<LocalModsContainer> GetLocalMods() {
                return GetInstalledAndSupportsMods().SelectMany(GetGameLocalMods);
            }

            IEnumerable<Game> GetInstalledAndSupportsMods() {
                return _games.Where(x => x.InstalledState.IsInstalled && x.SupportsMods());
            }

            static IEnumerable<LocalModsContainer> GetGameLocalMods(Game game) {
                var gp = game.InstalledState.Directory;
                var mp = game.Modding().ModPaths.Path;

                var list = new List<LocalModsContainer>();
                if (gp != null)
                    list.Add(new LocalModsContainer(game.MetaData.Name + " Game folder", gp.ToString(), game));

                if (mp != null && !Tools.FileUtil.ComparePathsOsCaseSensitive(gp.ToString(), mp.ToString()))
                    list.Add(new LocalModsContainer(game.MetaData.Name + " Mods", mp.ToString(), game));

                return list;
            }

            internal IEnumerable<GameController> GetControllers() {
                return GetInstalledAndSupportsMods().Select(x => x.Controller);
            }

            public ISupportModding GetSupportedGame(IMod mod) {
                return GetInstalledGames().FirstOrDefault(x => x.SupportsContent(mod));
            }
        }

        protected class Arma3ModListBuilder : ModListBuilder
        {
            const string AllInArmaA1Dummies = "A1Dummies";
            static readonly string[] aiaStandaloneMods = {
                "@AllInArmaStandaloneLite", "@AllInArmaStandalone", "@IFA3SA",
                "@IFA3SA_Lite"
            };
            static readonly string[] aiamods = {"@AllInArma"};
            static readonly string[] ifModFolders = {"@LIB_DLC_1", "@IF_Other_Addons", "@IF"};
            static readonly string[] ifMainModFolders = {"@IF", "@IFA3", "@IFA3M"};
            static readonly string[] ifModFoldersLite = ifModFolders.Select(x => x + "_Lite").ToArray();
            static readonly string[] ifMainModFoldersLite = ifMainModFolders.Select(x => x + "_Lite").ToArray();
            readonly AllInArmaGames _aiaGames;
            List<IAbsoluteDirectoryPath> _additional;
            List<IAbsoluteDirectoryPath> _additionalAiA;
            IfaState _ifa;
            IMod[] _nonAiaMods;

            public Arma3ModListBuilder(AllInArmaGames aiaGames) {
                _aiaGames = aiaGames;
            }

            protected override void ProcessMods() {
                _additional = new List<IAbsoluteDirectoryPath>();
                _additionalAiA = new List<IAbsoluteDirectoryPath>();
                _ifa = IfaState.None;
                _nonAiaMods = InputMods.Where(x => !IsAiaMod(x) && !IsAiaSAMod(x)).ToArray();
                ProcessIronFrontMods();
                AddPrimaryGameFolders();
                ProcessAndAddNormalModsWithAdditionalGameRequirements();
                AddNormalMods();
                ProcessAndAddAiaMods();
            }

            void AddNormalMods() {
                HandleA3Mp();
                OutputMods.AddRange(_nonAiaMods.SelectMany(x => x.GetPaths().OfType<IAbsoluteDirectoryPath>()));
            }

            void HandleA3Mp() {
                foreach (var a3Mp in Arma2TerrainPacks.Select(m => InputMods
                    .FirstOrDefault(x => x.Name.Equals(m, StringComparison.InvariantCultureIgnoreCase)))
                    .Where(a3Mp => a3Mp != null))
                    OutputMods.AddRange(a3Mp.GetPaths().OfType<IAbsoluteDirectoryPath>());
            }

            void ProcessAndAddNormalModsWithAdditionalGameRequirements() {
                foreach (var mod in _nonAiaMods)
                    ProcessModIfHasAdditionalGameRequirements(mod);
                OutputMods.AddRange(_additional);
            }

            void ProcessAndAddAiaMods() {
                foreach (var mod in InputMods.Where(IsAiaMod))
                    ProcessModIfHasAdditionalGameRequirements(mod);

                foreach (var mod in InputMods.Where(IsAiaSAMod))
                    ProcessAiaLiteMod(mod);

                // problematic?
                //OutputMods.RemoveAll(x => x..Split('/', '\\').First().Equals("@allinarma", StringComparison.CurrentCultureIgnoreCase));
                OutputMods.RemoveAll(x => _additionalAiA.Contains(x));
                OutputMods.AddRange(_additionalAiA);
            }

            void ProcessAiaLiteMod(IMod mod) {
                _additionalAiA.AddRange(mod.GetPaths().OfType<IAbsoluteDirectoryPath>());
                if (_ifa > IfaState.None)
                    HandleIfa();
            }

            void ProcessModIfHasAdditionalGameRequirements(IMod mod) {
                var requirements = mod.GetGameRequirements().Select(_aiaGames.Find).ToArray();
                if (!requirements.Any())
                    return;

                var existingGames = requirements.Where(x => x.InstalledState.IsInstalled);
                if (!aiamods.ContainsIgnoreCase(mod.Name))
                    ProcessNonAiaMods(mod, existingGames);
                else
                    HandleAiaMods(mod, existingGames);
            }

            static bool IsAiaMod(IMod x) {
                return aiamods.ContainsIgnoreCase(x.Name);
            }

            static bool IsAiaSAMod(IMod x) {
                return aiaStandaloneMods.ContainsIgnoreCase(x.Name);
            }

            void ProcessIronFrontMods() {
                if (InputMods.Any(x => ifMainModFolders.ContainsIgnoreCase(x.Name)))
                    _ifa = IfaState.Full;
                else if (InputMods.Any(x => ifMainModFoldersLite.ContainsIgnoreCase(x.Name)))
                    _ifa = IfaState.Lite;
                else
                    return;
                InputMods.RemoveAll(IsIronFrontFullOrLiteMod);
                var validGamePaths = GetValidGamePaths();
                OutputMods.AddRange(ExistingMods(validGamePaths, _ifa == IfaState.Lite ? ifModFoldersLite : ifModFolders));
                OutputMods.AddRange(ExistingMods(validGamePaths,
                    _ifa == IfaState.Lite ? ifMainModFoldersLite : ifMainModFolders));
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

            void ProcessNonAiaMods(IMod mod, IEnumerable<Game> existingGames) {
                foreach (var g in existingGames)
                    AddGameDefaultAndAdditionalModFolders(g, _additional);

                _additional.Add(Spec.GamePath);
                _additional.AddRange(mod.GetPaths().OfType<IAbsoluteDirectoryPath>());
            }

            static void AddGameDefaultAndAdditionalModFolders(Game game, List<IAbsoluteDirectoryPath> modList) {
                modList.Add(game.InstalledState.Directory);
                var md = game as ISupportModding;
                if (md != null)
                    modList.AddRange(md.GetAdditionalLaunchMods().OfType<IAbsoluteDirectoryPath>());
            }

            void HandleAiaMods(IMod mod, IEnumerable<Game> existingGames) {
                var aiaModPath = mod.Controller.Path;
                _additionalAiA.Add(CombineModPathIfNeeded(SubMod(mod, "ProductDummies")));

                HandleGames(existingGames, aiaModPath);

                _additionalAiA.Add(Spec.GamePath);
                _additionalAiA.Add(CombineModPathIfNeeded(SubMod(mod, "Core")));

                if (_ifa > IfaState.None)
                    HandleIfa();

                _additionalAiA.Add(CombineModPathIfNeeded(SubMod(mod, "PostA3")));
            }

            void HandleIfa() {
                var gamePaths = GetValidGamePaths();
                _additionalAiA.AddRange(ExistingMods(gamePaths, GetIFAMod("@IFA3M")));
                _additionalAiA.AddRange(ExistingMods(gamePaths, GetIFAMod("@IFA3")));
            }

            string GetIFAMod(string mod) {
                var lite = _ifa == IfaState.Lite ? "_Lite" : "";
                return mod + lite;
            }

            void HandleGames(IEnumerable<Game> existingGames, IAbsoluteDirectoryPath aiaModPath) {
                var list = existingGames.ToList();
                HandleArma1(list, aiaModPath);
                foreach (var g in list)
                    AddGameDefaultAndAdditionalModFolders(g, _additionalAiA);
            }

            void HandleArma1(ICollection<Game> existingGames, IAbsoluteDirectoryPath aiaModPath) {
                var a1 = existingGames.FirstOrDefault(g => g.Id == GameUuids.Arma1);
                if (a1 == null)
                    return;
                existingGames.Remove(a1);
                AddGameDefaultAndAdditionalModFolders(a1, _additionalAiA);
                _additionalAiA.Add(aiaModPath.GetChildDirectoryWithName(AllInArmaA1Dummies));
            }

            IAbsoluteDirectoryPath[] GetValidGamePaths() {
                return new[] {Spec.ModPath, Spec.GamePath}.Where(x => x != null).ToArray();
            }

            IAbsoluteDirectoryPath CombineModPathIfNeeded(string mod) {
                return Spec.ModPath == null
                    ? Spec.GamePath.GetChildDirectoryWithName(mod)
                    : Spec.ModPath.GetChildDirectoryWithName(mod);
            }

            static string SubMod(IMod mod, string name) {
                return mod.Name + "/" + name;
            }

            enum IfaState
            {
                None,
                Lite,
                Full
            }
        }
    }
}