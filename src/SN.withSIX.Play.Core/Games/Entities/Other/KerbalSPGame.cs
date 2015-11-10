// <copyright company="SIX Networks GmbH" file="KerbalSPGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class KerbalSPGame : Game, ISupportSteamSettings, ILaunchWith<IBasicGameLauncher>
    {
        static readonly SteamInfo steamInfo = new SteamInfo(220200, "Kerbal Space Program");
        static readonly GameMetaData metaData = new GameMetaData {
            Name = "Kerbal Space Program",
            ShortName = "KSP",
            Author = "Squad",
            Description =
                "KSP is a game where the players create and manage their own space program. Build spacecraft, fly them, and try to help the Kerbals to fulfill their ultimate mission of conquering space.",
            Slug = "kerbal-space-program",
            StoreUrl = "https://kerbalspaceprogram.com/kspstore/index.php?p=22".ToUri(),
            SupportUrl = "https://kerbalspaceprogram.com/".ToUri(),
            ReleasedOn = new DateTime(2011, 6, 24),
            IsFree = false
        };

        public KerbalSPGame(Guid id, GameSettingsController settingsController)
            : base(id, new KerbalSPSettings(id, new KerbalSPStartupParameters(), settingsController)) {}

        public override GameMetaData MetaData
        {
            get { return metaData; }
        }
        protected override SteamInfo SteamInfo
        {
            get { return steamInfo; }
        }
        public bool LaunchUsingSteam { get; set; }
        public bool ResetGameKeyEachLaunch { get; set; }

        protected override IAbsoluteFilePath GetExecutable() {
            return GetFileInGameDirectory("ksp.exe");
        }

        public override Task<IReadOnlyCollection<string>> ShortcutLaunchParameters(IGameLauncherFactory factory,
            string identifier) {
            throw new NotImplementedException();
        }

        public override Task<int> Launch(IGameLauncherFactory factory) {
            return LaunchBasic(factory.Create(this));
        }

        protected override string GetStartupLine() {
            return InstalledState.IsInstalled
                ? new[] {InstalledState.LaunchExecutable.ToString()}.Concat(Settings.StartupParameters.Get())
                    .CombineParameters()
                : string.Empty;
        }
    }

    public class KerbalSPSettings : GameSettings
    {
        public KerbalSPSettings(Guid gameId, KerbalSPStartupParameters sp, GameSettingsController controller)
            : base(gameId, sp, controller) {
            StartupParameters = sp;
        }

        public new KerbalSPStartupParameters StartupParameters { get; private set; }
    }

    public class UnityStartupParameters : GameStartupParameters
    {
        static readonly Regex spacedPropertyRegex = new Regex(@"""[-](\w+)=([^""]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex propertyRegex = new Regex(@"[-](\w+)=([^ ""]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex switchRegex = new Regex(@"[-]([^ ""=]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected UnityStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        [Category(GameSettingCategories.Game),
         Description(
             "Allow Game running even when its window does not have focus (i.e. running in the background)")
        ]
        public bool PopupWindow1198363464
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }

        protected override IEnumerable<string> BuildSwitches() {
            return SwitchStorage.Select(BuildSwitch);
        }

        static string BuildSwitch(string @switch) {
            return String.Format("-{0}", @switch.ToLower());
        }

        protected override IEnumerable<string> BuildParameters() {
            return Enumerable.Empty<string>();
        }

        protected override void ParseInputString(string input) {
            var switches = switchRegex.Matches(input);
            foreach (Match s in switches)
                SetSwitchOrDefault(true, s.Groups[1].Value, true);
        }
    }

    public class KerbalSPStartupParameters : UnityStartupParameters
    {
        public KerbalSPStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
    }
}