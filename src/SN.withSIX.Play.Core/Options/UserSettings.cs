// <copyright company="SIX Networks GmbH" file="UserSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using Caliburn.Micro;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Options.Entries;
using PropertyChangedBase = SN.withSIX.Core.Helpers.PropertyChangedBase;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "UserSettings", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class UserSettings : PropertyChangedBase, IEnableLogging
    {
        public static readonly string[] Locales = {"en", "de", "es", "fr", "nl", "ru"};
        [DataMember] AccountOptions _accountOptions = new AccountOptions();
        [DataMember] AppOptions _appOptions = new AppOptions();
        [DataMember] Version _appVersion;
        [DataMember] GameOptions _gameOptions = new GameOptions();
        [DataMember] Migrations _migrations = new Migrations();
        [DataMember] MissionOptions _missionOptions = new MissionOptions();
        [DataMember] ModOptions _modOptions = new ModOptions();
        bool _ready = true;
        [DataMember] ServerOptions _serverOptions = new ServerOptions();
        Subject<Unit> _subject;
        [DataMember] WindowSettings _windowSettings;

        public UserSettings() {
            Setup();
        }

        public IObservable<Unit> Changed
        {
            get { return _subject.AsObservable(); }
        }
        public WindowSettings WindowSettings
        {
            get { return _windowSettings; }
            set { _windowSettings = value; }
        }
        public Version AppVersion
        {
            get { return _appVersion; }
            set { _appVersion = value; }
        }
        public AppOptions AppOptions
        {
            get { return _appOptions ?? (_appOptions = new AppOptions()); }
            set { _appOptions = value; }
        }
        public GameOptions GameOptions
        {
            get { return _gameOptions ?? (_gameOptions = new GameOptions()); }
            set { _gameOptions = value; }
        }
        public Migrations Migrations
        {
            get { return _migrations ?? (_migrations = new Migrations()); }
            set { _migrations = value; }
        }
        public ModOptions ModOptions
        {
            get { return _modOptions ?? (_modOptions = new ModOptions()); }
            set { _modOptions = value; }
        }
        public ServerOptions ServerOptions
        {
            get { return _serverOptions ?? (_serverOptions = new ServerOptions()); }
            set { _serverOptions = value; }
        }
        public AccountOptions AccountOptions
        {
            get { return _accountOptions ?? (_accountOptions = new AccountOptions()); }
            set { _accountOptions = value; }
        }
        public MissionOptions MissionOptions
        {
            get { return _missionOptions ?? (_missionOptions = new MissionOptions()); }
            set { _missionOptions = value; }
        }
        public bool Ready
        {
            get { return _ready; }
            set { SetProperty(ref _ready, value); }
        }
        public Version OldVersion { get; set; }
        [DataMember]
        public bool ClearedAwesomium { get; set; }
        public Version Version { get; set; }

        public void RaiseChanged() {
            _subject.OnNext(Unit.Default);
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext sc) {
            _ready = true;
            Setup();
        }

        void Setup() {
            _subject = new Subject<Unit>();
            if (!Execute.InDesignMode) {
                AppOptions.WhenAnyValue(x => x.UseElevatedService)
                    .Subscribe(x => Common.Flags.UseElevatedService = x);
            }
            FixRecentModSets();
            AppOptions.TrySetProxy(AppOptions.HttpProxy);
        }

        [Obsolete]
        void FixRecentModSets() {
            if (ModOptions.RecentCollections.Contains(null)) {
                ModOptions.RecentCollections =
                    new ReactiveList<RecentCollection>(ModOptions.RecentCollections.Where(x => x != null));
            }
        }
    }
}