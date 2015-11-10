// <copyright company="SIX Networks GmbH" file="BasicGameStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SN.withSIX.Mini.Core.Games
{
    public abstract class BasicGameStartupParameters : GameStartupParameters
    {
        static readonly Regex spacedPropertyRegex = new Regex(@"[-](\w+) ""([^""]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex propertyRegex = new Regex(@"[-](\w+) ([^ ""]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex switchRegex = new Regex(@"[-]([-]?)([^ ""=]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected BasicGameStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}

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