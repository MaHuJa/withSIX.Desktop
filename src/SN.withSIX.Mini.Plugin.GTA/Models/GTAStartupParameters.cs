// <copyright company="SIX Networks GmbH" file="GTAStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    [DataContract]
    public abstract class GTAStartupParameters : GameStartupParameters
    {
        static readonly Regex propertyRegex = new Regex(
            @"(?<property>(?<![\w])[-](?<name>\w+) (?<value>(?=[^-])[^ ]+))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex switchRegex = new Regex(@"(?<switch>(?<![\w])[-](?<name>[^ ]+)(?![ ][\w]))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected GTAStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}

        protected override IEnumerable<string> BuildSwitches() {
            return SwitchStorage.Select(BuildSwitch);
        }

        static string BuildSwitch(string @switch) {
            return String.Format("-{0}", @switch.ToLower());
        }

        protected override IEnumerable<string> BuildParameters() {
            return ParameterStorage.Select(BuildParameter);
        }

        static string BuildParameter(KeyValuePair<string, string> setting) {
            return string.Format("-{0} {1}", setting.Key.ToLower(), setting.Value);
        }

        protected override void ParseInputString(string input) {
            var properties = propertyRegex.Matches(input);
            foreach (Match p in properties) {
                input = input.Replace(p.Groups[0].Value, String.Empty);
                SetPropertyOrDefault(CutdownOnTrailingBackslashes(p.Groups["value"].Value), p.Groups["name"].Value, true);
            }
            var switches = switchRegex.Matches(input);
            foreach (Match s in switches)
                SetSwitchOrDefault(true, s.Groups["name"].Value, true);
        }
    }
}