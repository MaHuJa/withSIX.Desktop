// <copyright company="SIX Networks GmbH" file="GameStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class GameStartupParameters : PropertyChangedBase
    {
        string _startupLine;
        [DataMember] public Dictionary<string, string> ParameterStorage = new Dictionary<string, string>();
        [DataMember] public IList<string> SwitchStorage = new List<string>();

        protected GameStartupParameters(params string[] defaultParameters) {
            DefaultParams = defaultParameters.CombineParameters();
            Parse(DefaultParams, true);
        }

        string DefaultParams { get; }
        [Browsable(false)]
        public string StartupLine
        {
            get { return _startupLine; }
            set { Parse(value); }
        }

        public virtual IEnumerable<string> Get() {
            return BuildParameters().Concat(BuildSwitches());
        }

        protected abstract IEnumerable<string> BuildSwitches();
        protected abstract IEnumerable<string> BuildParameters();

        protected string GetPropertyOrDefault([CallerMemberName] String key = null) {
            key = key.ToLower();
            return ParameterStorage.ContainsKey(key) ? ParameterStorage[key] : null;
        }

        protected bool GetSwitchOrDefault([CallerMemberName] String key = null) {
            key = key.ToLower();
            return SwitchStorage.Any(x => x == key);
        }

        protected void SetSwitchOrDefault(bool value, [CallerMemberName] string key = null, bool silent = false) {
            key = key.ToLower();

            if (value) {
                if (!SwitchStorage.None(x => x == key))
                    return;
                SwitchStorage.Add(key);
                if (silent)
                    return;
                OnPropertyChanged(key);
                UpdateStartupLine();
                return;
            }

            if (SwitchStorage.All(x => x != key))
                return;
            SwitchStorage.Remove(key);
            if (silent)
                return;
            OnPropertyChanged(key);
            UpdateStartupLine();
        }

        protected void SetPropertyOrDefault(string value, [CallerMemberName] string key = null, bool silent = false) {
            key = key.ToLower();

            var hasKey = ParameterStorage.ContainsKey(key);
            if (String.IsNullOrWhiteSpace(value)) {
                if (!hasKey)
                    return;
                ParameterStorage.Remove(key);
                if (silent)
                    return;
                OnPropertyChanged(key);
                UpdateStartupLine();
                return;
            }

            if (hasKey) {
                if (ParameterStorage[key] == value)
                    return;
                ParameterStorage[key] = value;
                if (silent)
                    return;
                OnPropertyChanged(key);
                UpdateStartupLine();
                return;
            }

            ParameterStorage.Add(key, value);
            if (silent)
                return;
            OnPropertyChanged(key);
            UpdateStartupLine();
        }

        internal void Parse(string input, bool silent = false) {
            ParameterStorage = new Dictionary<string, string>();
            SwitchStorage = new List<string>();

            ParseInputString(input);

            UpdateStartupLine();
            if (!silent)
                Refresh();
        }

        protected abstract void ParseInputString(string input);

        protected static string CutdownOnTrailingBackslashes(string value) {
            var endsWithQuote = value.EndsWith("\"");

            if (endsWithQuote)
                value = value.Substring(0, value.Length - 1);

            var trail = (endsWithQuote ? "\"" : null);
            if (value.EndsWith("\\"))
                return value.TrimEnd('\\') + "\\" + trail;
            return value + trail;
        }

        void UpdateStartupLine() {
            _startupLine = Get().CombineParameters();
            OnPropertyChanged("StartupLine");
        }
    }
}