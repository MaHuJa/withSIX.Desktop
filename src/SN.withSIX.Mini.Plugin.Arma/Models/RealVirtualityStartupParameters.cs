// <copyright company="SIX Networks GmbH" file="RealVirtualityStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [DataContract]
    public abstract class RealVirtualityStartupParameters : GameStartupParameters
    {
        static readonly Regex spacedPropertyRegex = new Regex(@"""[-](\w+)=([^""]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex propertyRegex = new Regex(@"[-](\w+)=([^ ""]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex switchRegex = new Regex(@"[-]([^ ""=]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected RealVirtualityStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}

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
                SetSwitchOrDefault(true, s.Groups[1].Value, true);
        }

        protected override IEnumerable<string> BuildSwitches() {
            return SwitchStorage.Select(BuildSwitch);
        }

        static string BuildSwitch(string @switch) {
            return string.Format("-{0}", @switch.ToLower());
        }

        protected override IEnumerable<string> BuildParameters() {
            return ParameterStorage.Select(BuildParameter);
        }

        static string BuildParameter(KeyValuePair<string, string> setting) {
            return string.Format("-{0}={1}", setting.Key.ToLower(), setting.Value);
        }
    }
}