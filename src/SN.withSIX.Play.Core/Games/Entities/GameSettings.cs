// <copyright company="SIX Networks GmbH" file="GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public abstract class GameSettings : PropertyChangedBase
    {
        readonly GameSettingsController _controller;
        readonly Guid _gameId;
        int? _keepLatestVersions;

        protected GameSettings(Guid gameId, GameStartupParameters sp, GameSettingsController controller) {
            _gameId = gameId;
            _controller = controller;
            StartupParameters = sp;

            controller.Register(gameId, this);

            if (StartupLine != null)
                sp.Parse(StartupLine);

            StartupParameters.WhenAnyValue(x => x.StartupLine)
                .Subscribe(x => StartupLine = x);

            this.WhenAnyValue(x => x.DefaultDirectory)
                .Where(x => Directory == null && x != null)
                .Subscribe(x => { Directory = x; });
        }

        protected int MigrationVersion
        {
            get { return GetIntValue("__MigrateVersion"); }
            set { SetIntValue(value, "__MigrateVersion"); }
        }
        public Guid CurrentProfile
        {
            get { return _controller.ActiveProfile.Id; }
        }
        public IAbsoluteDirectoryPath Directory
        {
            get { return GetValue<string>().ToAbsoluteDirectoryPathNullSafe(); }
            set
            {
                if (value == null)
                    value = DefaultDirectory;
                SetValue(value == null ? null : value.ToString());
            }
        }
        public ProcessPriorityClass Priority
        {
            get { return GetNullableEnum<ProcessPriorityClass?>().GetValueOrDefault(ProcessPriorityClass.Normal); }
            set { SetEnum(value); }
        }
        public bool LaunchAsAdministrator
        {
            get { return GetBoolValue(); }
            set { SetBoolValue(value); }
        }
        public bool InjectSteam
        {
            get { return GetBoolValue(); }
            set { SetBoolValue(value); }
        }
        public string StartupLine
        {
            get { return GetValue<string>(); }
            private set { SetValue(value); }
        }
        public int? KeepLatestVersions
        {
            get { return _keepLatestVersions; }
            set { SetProperty(ref _keepLatestVersions, value); }
        }
        public GameStartupParameters StartupParameters { get; }
        public RecentGameSettings Recent
        {
            get { return GetValue<RecentGameSettings>(); }
            private set { SetValue(value); }
        }
        public IAbsoluteDirectoryPath DefaultDirectory { get; set; }

        public virtual void Migrate(int version) {
            MigrationVersion = version;
        }

        public void RefreshInfo() {
            StartupParameters.Parse(StartupLine ?? StartupParameters.DefaultParams);
            Refresh();
        }

        public void Save() {
            _controller.Save();
        }

        protected bool SetValue<T>(T value, [CallerMemberName] String propertyName = null) where T : class {
            var changed = _controller.SetValue(_gameId, propertyName, value);
            if (changed)
                OnPropertyChanged(propertyName);

            return changed;
        }

        protected bool SetEnum<T>(T value, [CallerMemberName] String propertyName = null) where T : struct, IConvertible {
            var changed = _controller.SetValue(_gameId, propertyName, value);
            if (changed)
                OnPropertyChanged(propertyName);

            return changed;
        }

        /// <summary>
        ///     Special variant of SetValue as we require to use nullable bools for the passthrough system to work...
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetBoolValue(bool? value, [CallerMemberName] String propertyName = null) {
            var changed = _controller.SetValue(_gameId, propertyName, value);
            if (changed)
                OnPropertyChanged(propertyName);

            return changed;
        }

        protected bool SetIntValue(int value, [CallerMemberName] String propertyName = null) {
            var changed = _controller.SetValue(_gameId, propertyName, value);
            if (changed)
                OnPropertyChanged(propertyName);

            return changed;
        }

        protected T GetValue<T>([CallerMemberName] String propertyName = null) where T : class {
            return _controller.GetValue<T>(_gameId, propertyName);
        }

        protected T GetEnum<T>([CallerMemberName] String propertyName = null) where T : struct, IConvertible {
            return _controller.GetValue<T>(_gameId, propertyName);
        }

        protected T GetNullableEnum<T>([CallerMemberName] String propertyName = null) {
            return _controller.GetValue<T>(_gameId, propertyName);
        }

        protected int GetIntValue([CallerMemberName] String propertyName = null) {
            return _controller.GetValue<int>(_gameId, propertyName);
        }

        /// <summary>
        ///     Special variant of GetValue as we require to use nullable bools for the passthrough system to work...
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool GetBoolValue(bool defaultValue = false, [CallerMemberName] String propertyName = null) {
            return _controller.GetValue<bool?>(_gameId, propertyName).GetValueOrDefault(defaultValue);
        }

        public void Register(GameSettingsProfile profile) {
            profile.Setup(_gameId);
        }
    }

    public abstract class GameStartupParameters : PropertyChangedBase
    {
        string _startupLine;
        protected Dictionary<string, string> ParameterStorage = new Dictionary<string, string>();
        protected IList<string> SwitchStorage = new List<string>();

        protected GameStartupParameters(params string[] defaultParameters) {
            DefaultParams = defaultParameters.CombineParameters();
            Parse(DefaultParams, true);
        }

        public string DefaultParams { get; }
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