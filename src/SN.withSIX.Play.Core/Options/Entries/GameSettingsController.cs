// <copyright company="SIX Networks GmbH" file="GameSettingsController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using AutoMapper;
using AutoMapper.Mappers;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Entities.Other;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    [KnownType(typeof (GlobalGameSettingsProfile)), KnownType(typeof (GameSettingsProfile)),
     KnownType(typeof (RecentGameSettings)), KnownType(typeof (ServerQueryMode)),
     KnownType(typeof (ProcessPriorityClass))]
    public class GameSettingsController : PropertyChangedBase
    {
        const int MigrationVersion = 1;
        static readonly MappingEngine legacyEngine = new MappingEngine(GetConfiguration());
        GameSettingsProfileBase _activeProfile;
        [DataMember] Guid? _activeProfileGuid;
        List<GameSettings> _gameSettings = new List<GameSettings>();
        [DataMember] ReactiveList<GameSettingsProfileBase> _profiles;

        public GameSettingsController() {
            _profiles = new ReactiveList<GameSettingsProfileBase> {new GlobalGameSettingsProfile()};
            _activeProfile = _profiles.First();
            Profiles = _profiles.CreateDerivedCollection(x => x);
            SetupRefresh();
        }

        public IReactiveDerivedList<GameSettingsProfileBase> Profiles { get; set; }
        public GameSettingsProfileBase ActiveProfile
        {
            get { return _activeProfile; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                SetProperty(ref _activeProfile, value);
            }
        }

        public void Save() {
            DomainEvilGlobal.Settings.RaiseChanged();
        }

        public GameSettingsProfileBase CreateProfile(string name, string color, Guid parentId) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(color));
            Contract.Requires<ArgumentNullException>(parentId != Guid.Empty);

            var parent = Profiles.First(x => x.Id == parentId);

            if (_profiles.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                throw new ProfileWithSameNameAlreadyExistsException();

            var profile = new GameSettingsProfile(name, color, parent);
            foreach (var gs in _gameSettings)
                gs.Register(profile);

            _profiles.Add(profile);
            ActiveProfile = profile;

            return profile;
        }

        public void DeleteProfile(Guid uuid) {
            Contract.Requires<ArgumentNullException>(uuid != Guid.Empty);

            if (ActiveProfile.Id == uuid)
                ActiveProfile = _profiles.First();

            var profile = _profiles.First(x => x.Id == uuid);
            _profiles.Remove(profile);
            foreach (var p in _profiles.Where(p => profile.Parent == profile))
                p.Parent = null;
        }

        public void ActivateProfile(Guid uuid) {
            Contract.Requires<ArgumentNullException>(uuid != Guid.Empty);

            ActiveProfile = _profiles.First(x => x.Id == uuid);
        }

        public void ActivateProfile(string name) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

            var profile = _profiles.First(x => x.Name == name);
            ActiveProfile = profile;
        }

        void SetupGame(Guid gameId) {
            foreach (var p in _profiles)
                p.Setup(gameId);
        }

        public T GetValue<T>(Guid game, string propertyName) {
            return
                GetAllProfiles(ActiveProfile).Select(x => x.GetData<T>(game, propertyName))
                    .FirstOrDefault(x => !EqualityComparer<T>.Default.Equals(x, default(T)));
        }

        static IEnumerable<IGetData> GetAllProfiles(GameSettingsProfileBase activeProfile) {
            Contract.Requires<NullReferenceException>(activeProfile != null);

            var profile = activeProfile;
            while (profile != null) {
                yield return profile;
                profile = profile.Parent;
            }
        }

        public bool SetValue<T>(Guid gameId, string propertyName, T value) {
            return ActiveProfile.SetData(gameId, propertyName, value);
        }

        public void Register(Guid gameId, GameSettings gameSettings) {
            _gameSettings.Add(gameSettings);
            SetupGame(gameId);
            MigrateOldGameSettings(gameId, gameSettings);
            gameSettings.Migrate(MigrationVersion);
        }

        void MigrateOldGameSettings(Guid gameId, GameSettings gameSettings) {
            if (gameId == GameUuids.Arma2Co)
                ProcessGame(GameUuids.Arma2Oa, gameSettings);
            ProcessGame(gameId, gameSettings);
        }

        void ProcessGame(Guid gameId, GameSettings gameSettings) {
            var oldGameSettings = DomainEvilGlobal.Settings.GameOptions.GetLegacyGameSettings(gameId);
            if (oldGameSettings != null)
                MigrateOldGameSettings(oldGameSettings, gameSettings);
            var oldGameSetSettings = DomainEvilGlobal.Settings.GameOptions.GetLegacyGameSetSettings(gameId);
            if (oldGameSetSettings != null)
                MigrateOldGameSetSettings(oldGameSetSettings, gameSettings);
        }

        static ConfigurationStore GetConfiguration() {
            var config = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            config.SetupConverters();

            config.CreateMap<LegacyGameSettings, GameSettings>()
                .ForMember(x => x.Directory, opt => opt.ResolveUsing(src => MigratePath(src.GamePath)))
                .ForMember(x => x.StartupParameters, opt => opt.Ignore())
                .AfterMap(AfterMap)
                .Include<LegacyGameSettings, RealVirtualitySettings>()
                .Include<LegacyGameSettings, Homeworld2Settings>();

            config.CreateMap<LegacyGameSettings, Homeworld2Settings>()
                .ForMember(x => x.RepositoryDirectory, opt => opt.ResolveUsing(src => MigratePath(src.SynqPath)))
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSettings, RealVirtualitySettings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore())
                .Include<LegacyGameSettings, ArmaSettings>()
                .Include<LegacyGameSettings, CarrierCommandSettings>()
                .Include<LegacyGameSettings, TakeOnMarsSettings>()
                .Include<LegacyGameSettings, Arma2FreeSettings>()
                .Include<LegacyGameSettings, DayZSettings>();

            config.CreateMap<LegacyGameSettings, CarrierCommandSettings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSettings, TakeOnMarsSettings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSettings, DayZSettings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSettings, Arma2FreeSettings>()
                .ForMember(x => x.RepositoryDirectory, opt => opt.ResolveUsing(src => MigratePath(src.SynqPath)))
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSettings, ArmaSettings>()
                .ForMember(x => x.ModDirectory, opt => opt.ResolveUsing(src => MigratePath(src.ModPath)))
                .ForMember(x => x.RepositoryDirectory, opt => opt.ResolveUsing(src => MigratePath(src.SynqPath)))
                .ForMember(x => x.StartupParameters, opt => opt.Ignore())
                .Include<LegacyGameSettings, Arma2OaSettings>();

            config.CreateMap<LegacyGameSettings, Arma2OaSettings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore())
                .Include<LegacyGameSettings, Arma2CoSettings>()
                .Include<LegacyGameSettings, Arma3Settings>();

            config.CreateMap<LegacyGameSettings, Arma2CoSettings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSettings, Arma3Settings>()
                .ForMember(x => x.StartupParameters, opt => opt.Ignore());

            config.CreateMap<LegacyGameSetSettings, RecentGameSettings>()
                .ForMember(x => x.Mission, opt => opt.MapFrom(src => src.RecentMission))
                .ForMember(x => x.Collection, opt => opt.MapFrom(src => src.RecentCollection))
                .ForMember(x => x.Server, opt => opt.MapFrom(src => src.RecentServer));

            config.Seal();
            return config;
        }

        static IAbsoluteDirectoryPath MigratePath(string path) {
            return path == null || !path.IsValidAbsoluteDirectoryPath()
                ? null
                : path.TrimEnd('\\').ToAbsoluteDirectoryPath();
        }

        static void AfterMap(LegacyGameSettings src, GameSettings dst) {
            var startupParameters = src.StartupParameters;
            if (startupParameters != null)
                dst.StartupParameters.StartupLine = startupParameters;
        }

        void MigrateOldGameSetSettings(LegacyGameSetSettings oldGameSetSettings, GameSettings gameSettings) {
            legacyEngine.Map(oldGameSetSettings, gameSettings.Recent, oldGameSetSettings.GetType(),
                gameSettings.Recent.GetType());
        }

        void MigrateOldGameSettings(LegacyGameSettings oldGameSettings, GameSettings gameSettings) {
            legacyEngine.Map(oldGameSettings, gameSettings, oldGameSettings.GetType(), gameSettings.GetType());
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context) {
            _activeProfileGuid = _activeProfile == null ? (Guid?) null : _activeProfile.Id;
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            _gameSettings = new List<GameSettings>();
            if (_profiles.All(x => x.Id != GlobalGameSettingsProfile.GlobalId))
                _profiles.Insert(0, new GlobalGameSettingsProfile());
            var globalProfile = _profiles.First(x => x.Id == GlobalGameSettingsProfile.GlobalId);
            Profiles = _profiles.CreateDerivedCollection(x => x);
            _activeProfile = _profiles.First(x => x.Id == _activeProfileGuid);
            foreach (var p in _profiles.Where(p => p.Id != GlobalGameSettingsProfile.GlobalId))
                p.Parent = _profiles.FirstOrDefault(x => x.Id == p.ParentId) ?? globalProfile;

            SetupRefresh();
        }

        void SetupRefresh() {
            this.WhenAnyValue(x => x.ActiveProfile)
                .Skip(1)
                .Subscribe(x => {
                    foreach (var gs in _gameSettings)
                        gs.RefreshInfo();
                });
        }
    }

    public class ProfileWithSameNameAlreadyExistsException : Exception {}
}