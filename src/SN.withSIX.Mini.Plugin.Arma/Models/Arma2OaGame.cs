// <copyright company="SIX Networks GmbH" file="Arma2OaGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Plugin.Arma.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    /*
    [Game(GameUUids.Arma2Oa, Name = "Arma 2: Operation Arrowhead", Slug = "Arma-2",
        Executables = new[] {"arma2oa.exe"},
        ServerExecutables = new[] {"arma2oaserver.exe"},
        IsPublic = true,
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer},
        Dlcs = new[]{"BAF", "PMC", "ACR"})
    ]*/

    [RegistryInfo(BohemiaStudioRegistry + @"\ArmA 2 OA", "main")]
    [RvProfileInfo("Arma 2", "Arma 2 other profiles",
        "ArmA2OaProfile")]
    [SteamInfo(33930, "Arma 2 Operation Arrowhead")]
    [DataContract]
    public abstract class Arma2OaGame : Arma2Game
    {
        static readonly IReadOnlyCollection<string> defaultModFolders = new[] {"expansion"};
        protected Arma2OaGame(Guid id) : this(id, new Arma2OaGameSettings()) {}
        public Arma2OaGame(Guid id, Arma2OaGameSettings settings) : base(id, settings) {}
        protected override StartupBuilder GetStartupBuilder() => new StartupBuilder(this, new Arma2OaModListBuilder());

        protected override IEnumerable<IAbsoluteDirectoryPath> GetAdditionalLaunchMods()
            => defaultModFolders.Select(x => InstalledState.Directory.GetChildDirectoryWithName(x));

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
                if (!InputMods.Any(x => ifMainModFolders.ContainsIgnoreCase(x.PackageName)))
                    return;
                InputMods.RemoveAll(IsIronFrontFullOrLiteMod);
                OutputMods.AddRange(ExistingMods(GetOaPaths().Where(x => x != null).ToArray(), ifModFolders));
            }

            IEnumerable<IAbsoluteDirectoryPath> GetOaPaths() {
                return new[] {Spec.ModPath, Spec.GamePath};
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
        }
    }
}