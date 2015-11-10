// <copyright company="SIX Networks GmbH" file="Homeworld2Settings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy.Arma;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities.Other
{
    public class Homeworld2Settings : GameSettings
    {
        public Homeworld2Settings(Guid gameId, Homeworld2StartupParameters startupParameters,
            GameSettingsController controller) : base(gameId, startupParameters, controller) {
            StartupParameters = startupParameters;
        }

        public new Homeworld2StartupParameters StartupParameters { get; private set; }
        public IAbsoluteDirectoryPath RepositoryDirectory
        {
            get { return GetValue<string>().ToAbsoluteDirectoryPathNullSafe(); }
            set { SetValue(value == null ? null : value.ToString()); }
        }
    }

    public class Homeworld2StartupParameters : GameStartupParameters
    {
        static readonly Regex spacedPropertyRegex = new Regex(@"[-](\w+) ""([^""]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex propertyRegex = new Regex(@"[-](\w+) ([^ ""]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex switchRegex = new Regex(@"[-]([-]?)([^ ""=]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public Homeworld2StartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        [Category(GameSettingCategories.Graphics)]
        [Description("Width of screen in pixels")]
        [DisplayName("Screen Width")]
        public string w
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Graphics)]
        [Description("Height of screen in pixels")]
        [DisplayName("Screen Height")]
        public string h
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        public bool OverrideBigFile
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool LuaTrace
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Graphics)]
        public bool Windowed
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool QuickLoad
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool FreeMouse
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoInt3
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSBW
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSBoth
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSTGA
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSJPG
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SSNoLogo
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool SuperTurbo
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoS3TC
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoDisplayLists
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoPBuffer
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoSound
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoRender
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadNoROT
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadPreferROT
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadMostRecent
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool TexLoadAlwaysROT
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool HardwareCursor
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoVideoErrors
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool NoMovies
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        public bool silentErrors
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }

        protected override IEnumerable<string> BuildSwitches() {
            return SwitchStorage.Select(BuildSwitch);
        }

        static string BuildSwitch(string @switch) {
            return string.Format("-{0}", @switch.ToLower());
        }

        protected override IEnumerable<string> BuildParameters() {
            return ParameterStorage.Select(BuildParameter)
                .Aggregate((IEnumerable<string>) new String[0], (current, pars) => current.Concat(pars));
        }

        static IEnumerable<string> BuildParameter(KeyValuePair<string, string> setting) {
            return new[] {"-" + setting.Key.ToLower(), setting.Value};
        }

        protected override void ParseInputString(string input) {
            var properties = spacedPropertyRegex.Matches(input);
            foreach (Match p in properties) {
                input = input.Replace(p.Groups[0].Value, String.Empty);
                SetPropertyOrDefault(CutdownOnTrailingBackslashes(p.Groups[2].Value), p.Groups[1].Value, true);
            }

            properties = propertyRegex.Matches(input);
            foreach (Match p in properties) {
                input = input.Replace(p.Groups[0].Value, String.Empty);
                SetPropertyOrDefault(CutdownOnTrailingBackslashes(p.Groups[2].Value), p.Groups[1].Value, true);
            }

            var switches = switchRegex.Matches(input);
            foreach (Match s in switches)
                SetSwitchOrDefault(true, s.Groups[1].Value + s.Groups[2].Value, true);
        }
    }
}