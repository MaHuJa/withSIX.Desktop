// <copyright company="SIX Networks GmbH" file="Arma3Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Services;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [Game(GameUUids.Arma3, Name = "Arma 3", Slug = "Arma-3",
        Executables = new[] {"arma3.exe"},
        IsPublic = true,
        ServerExecutables = new[] {"arma3server.exe"},
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer},
        Dlcs = new [] { "Karts", "Helicopters", "Marksmen" })]
    [SteamInfo(107410, "Arma 3", DRM = true)]
    [RegistryInfo(BohemiaRegistry + @"\ArmA 3", "main")]
    [RvProfileInfo("Arma 3", "Arma 3 - other profiles",
        "Arma3Profile")]
    [SynqRemoteInfo("1ba63c97-2a18-42a7-8380-70886067582e", "82f4b3b2-ea74-4a7c-859a-20b425caeadb" /*GameUUids.Arma3 */)
    ]
    [DataContract]
    public class Arma3Game : ArmaGame
    {
        const string BattleEyeExe = "arma3battleye.exe";
        public static readonly string[] Arma2TerrainPacks = {
            "@a3mp", "@AllInArmaTerrainPack",
            "@AllInArmaTerrainPackLite"
        };
        Arma3GameSettings _settings;
        protected Arma3Game(Guid id) : this(id, new Arma3GameSettings()) {}

        public Arma3Game(Guid id, Arma3GameSettings settings) : base(id, settings) {
            _settings = settings;
        }

        protected override StartupBuilder GetStartupBuilder() => new StartupBuilder(this, new Arma3ModListBuilder());

        protected override IAbsoluteFilePath GetLaunchExecutable() {
            var battleEyeClientExectuable = GetBattleEyeClientExectuable();
            return /*Settings.ServerMode ||*/ !_settings.LaunchThroughBattlEye || !battleEyeClientExectuable.Exists
                ? base.GetLaunchExecutable()
                : battleEyeClientExectuable;
        }

        protected IAbsoluteFilePath GetBattleEyeClientExectuable() {
            return GetExecutable().GetBrotherFileWithName(BattleEyeExe);
        }

        protected override async Task<IEnumerable<string>> GetStartupParameters(IRealVirtualityLauncher launcher,
            ILaunchContentAction<IContent> action) {
            var defParams = await base.GetStartupParameters(launcher, action).ConfigureAwait(false);
            return /*Settings.ServerMode ||*/ !_settings.LaunchThroughBattlEye || !GetBattleEyeClientExectuable().Exists
                ? defParams
                : AddBattleEyeLaunchParameters(defParams);
        }

        static List<string> AddBattleEyeLaunchParameters(IEnumerable<string> defParams) {
            var sParams = defParams.ToList();
            sParams.Insert(0, "2");
            sParams.Insert(1, "1");
            sParams.Insert(2, "1");
            return sParams;
        }

        public override IReadOnlyCollection<Guid> GetCompatibleGameIds() {
            return new[] {Id, GameUuids.Arma2Co};
        }

        public class AllInArmaGames
        {
            readonly Game[] _games;
            readonly Game[] _gamesSupportModding;
            internal readonly Arma1Game Arma1;
            internal readonly Arma2Game Arma2;
            //internal readonly Arma2FreeGame Arma2Free;
            internal readonly Arma2OaGame Arma2Oa;
            internal readonly TakeOnHelicoptersGame TakeOn;

            public AllInArmaGames(Arma1Game arma1Game, Arma2Game arma2Game, //Arma2FreeGame arma2FreeGame,
                Arma2OaGame arma2OaGame,
                TakeOnHelicoptersGame takeOnHelicoptersGame) {
                Arma1 = arma1Game;
                Arma2 = arma2Game;
                //Arma2Free = arma2FreeGame;
                Arma2Oa = arma2OaGame;
                TakeOn = takeOnHelicoptersGame;
                _games = new Game[] {arma1Game, arma2Game, arma2OaGame, takeOnHelicoptersGame}; // arma2FreeGame, 
                _gamesSupportModding = new Game[] {arma1Game, arma2Game, arma2OaGame, takeOnHelicoptersGame};
                //_games.OfType<ISupportModding>().ToArray();
            }

            internal Game Find(Guid id) {
                return _games.First(x => x.Id == id);
            }
        }

        protected class Arma3ModListBuilder : ModListBuilder
        {
            const string AllInArmaA1Dummies = "A1Dummies";
            static readonly string[] aiaStandaloneMods = {
                "@AllInArmaStandaloneLite", "@AllInArmaStandalone", "@IFA3SA",
                "@IFA3SA_Lite"
            };
            //static readonly string[] aiamods = {"@AllInArma"};
            static readonly string[] ifModFolders = {"@LIB_DLC_1", "@IF_Other_Addons", "@IF"};
            static readonly string[] ifMainModFolders = {"@IF", "@IFA3", "@IFA3M"};
            static readonly string[] ifModFoldersLite = ifModFolders.Select(x => x + "_Lite").ToArray();
            static readonly string[] ifMainModFoldersLite = ifMainModFolders.Select(x => x + "_Lite").ToArray();
            //readonly AllInArmaGames _aiaGames;
            List<IAbsoluteDirectoryPath> _additional;
            List<IAbsoluteDirectoryPath> _additionalAiA;
            IfaState _ifa;
            IReadOnlyCollection<IModContent> _nonAiaMods;
            // we dont need to support legacy AiA anymore with standalone available..
            /*
            public Arma3ModListBuilder(AllInArmaGames aiaGames)
            {
                _aiaGames = aiaGames;
            }*/

            protected override void ProcessMods() {
                _additional = new List<IAbsoluteDirectoryPath>();
                _additionalAiA = new List<IAbsoluteDirectoryPath>();
                _ifa = IfaState.None;
                _nonAiaMods = InputMods.Where(x => !IsAiaSAMod(x)).ToArray(); // !IsAiaMod(x) && 
                ProcessIronFrontMods();
                AddPrimaryGameFolders();
                ProcessAndAddNormalModsWithAdditionalGameRequirements();
                AddNormalMods();
                ProcessAndAddAiaMods();
            }

            void AddNormalMods() {
                HandleA3Mp();
                OutputMods.AddRange(_nonAiaMods.SelectMany(GetModPaths));
            }

            void HandleA3Mp() {
                foreach (var a3Mp in Arma2TerrainPacks.Select(m => InputMods
                    .FirstOrDefault(x => x.PackageName.Equals(m, StringComparison.InvariantCultureIgnoreCase)))
                    .Where(a3Mp => a3Mp != null))
                    OutputMods.AddRange(GetModPaths(a3Mp));
            }

            void ProcessAndAddNormalModsWithAdditionalGameRequirements() {
                //foreach (var mod in _nonAiaMods)
                //  ProcessModIfHasAdditionalGameRequirements(mod);
                OutputMods.AddRange(_additional);
            }

            void ProcessAndAddAiaMods() {
                //foreach (var mod in InputMods.Where(IsAiaMod))
                //ProcessModIfHasAdditionalGameRequirements(mod);

                foreach (var mod in InputMods.Where(IsAiaSAMod))
                    ProcessAiaLiteMod(mod);

                // problematic?
                //OutputMods.RemoveAll(x => x..Split('/', '\\').First().Equals("@allinarma", StringComparison.CurrentCultureIgnoreCase));
                OutputMods.RemoveAll(x => _additionalAiA.Contains(x));
                OutputMods.AddRange(_additionalAiA);
            }

            void ProcessAiaLiteMod(IModContent mod) {
                _additionalAiA.AddRange(GetModPaths(mod));
                if (_ifa > IfaState.None)
                    HandleIfa();
            }

            /*
        void ProcessModIfHasAdditionalGameRequirements(IModContent mod) {
            var requirements = mod.GetGameRequirements().Select(_aiaGames.Find).ToArray();
            if (!requirements.Any())
                return;

            var existingGames = requirements.Where(x => x.InstalledState.IsInstalled);
            if (!aiamods.ContainsIgnoreCase(mod.PackageName))
                ProcessNonAiaMods(mod, existingGames);
            else
                HandleAiaMods(mod, existingGames);
        }
                
            static bool IsAiaMod(IModContent x) {
                return aiamods.ContainsIgnoreCase(x.PackageName);
            }
                */

            static bool IsAiaSAMod(IModContent x) {
                return aiaStandaloneMods.ContainsIgnoreCase(x.PackageName);
            }

            void ProcessIronFrontMods() {
                if (InputMods.Any(x => ifMainModFolders.ContainsIgnoreCase(x.PackageName)))
                    _ifa = IfaState.Full;
                else if (InputMods.Any(x => ifMainModFoldersLite.ContainsIgnoreCase(x.PackageName)))
                    _ifa = IfaState.Lite;
                else
                    return;
                InputMods.RemoveAll(IsIronFrontFullOrLiteMod);
                var validGamePaths = GetValidGamePaths();
                OutputMods.AddRange(ExistingMods(validGamePaths, _ifa == IfaState.Lite ? ifModFoldersLite : ifModFolders));
                OutputMods.AddRange(ExistingMods(validGamePaths,
                    _ifa == IfaState.Lite ? ifMainModFoldersLite : ifMainModFolders));
            }

            static bool IsIronFrontFullOrLiteMod(IModContent x) {
                return ifMainModFolders.ContainsIgnoreCase(x.PackageName) ||
                       ifMainModFoldersLite.ContainsIgnoreCase(x.PackageName) ||
                       ifModFolders.ContainsIgnoreCase(x.PackageName)
                       || ifModFoldersLite.ContainsIgnoreCase(x.PackageName);
            }

            static IEnumerable<IAbsoluteDirectoryPath> ExistingMods(IAbsoluteDirectoryPath[] paths, params string[] mods) {
                return paths.Any()
                    ? mods.Select(
                        x => paths.Select(path => path.GetChildDirectoryWithName(x)).FirstOrDefault(p => p.Exists))
                        .Where(x => x != null)
                    : Enumerable.Empty<IAbsoluteDirectoryPath>();
            }

            /*
            void ProcessNonAiaMods(IModContent mod, IEnumerable<Game> existingGames) {
                foreach (var g in existingGames)
                    AddGameDefaultAndAdditionalModFolders(g, _additional);

                _additional.Add(Spec.GamePath);
                _additional.AddRange(GetModPaths(mod));
            }

            static void AddGameDefaultAndAdditionalModFolders(Game game, List<IAbsoluteDirectoryPath> modList) {
                modList.Add(game.InstalledState.Directory);
                var md = game as ISupportModding;
                if (md != null)
                  modList.AddRange(md.GetAdditionalLaunchMods().OfType<IAbsoluteDirectoryPath>());
            }

            void HandleAiaMods(IModContent mod, IEnumerable<Game> existingGames) {
                //var aiaModPath = mod.Controller.Path;
                _additionalAiA.Add(CombineModPathIfNeeded(SubMod(mod, "ProductDummies")));

                //HandleGames(existingGames, aiaModPath);

                _additionalAiA.Add(Spec.GamePath);
                _additionalAiA.Add(CombineModPathIfNeeded(SubMod(mod, "Core")));

                if (_ifa > IfaState.None)
                    HandleIfa();

                _additionalAiA.Add(CombineModPathIfNeeded(SubMod(mod, "PostA3")));
            }
            */

            void HandleIfa() {
                var gamePaths = GetValidGamePaths();
                _additionalAiA.AddRange(ExistingMods(gamePaths, GetIFAMod("@IFA3M")));
                _additionalAiA.AddRange(ExistingMods(gamePaths, GetIFAMod("@IFA3")));
            }

            string GetIFAMod(string mod) {
                var lite = _ifa == IfaState.Lite ? "_Lite" : "";
                return mod + lite;
            }

            /*
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
            */

            IAbsoluteDirectoryPath[] GetValidGamePaths() {
                return new[] {Spec.ModPath, Spec.GamePath}.Where(x => x != null).ToArray();
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