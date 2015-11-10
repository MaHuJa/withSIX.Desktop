// <copyright company="SIX Networks GmbH" file="GameOptions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Options.Entries;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "GameOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class GameOptions : OptionBase
    {
        static Type[] _knownTypes;
        [DataMember] bool _CloseOnLaunch;
        [DataMember] Dictionary<string, string> _desiredPackageVersions = new Dictionary<string, string>();
        [DataMember] List<FavoriteDlc> _favoriteDlcs = new List<FavoriteDlc>();
        [DataMember] List<FavoriteGame> _favorites = new List<FavoriteGame>();
        [DataMember] GameFilter _Filter = new GameFilter();
        [DataMember] List<LegacyGameSetSettings> _gameSetSettings = new List<LegacyGameSetSettings>();
        [DataMember] List<LegacyGameSettings> _gameSettings = new List<LegacyGameSettings>();
        [DataMember] GameSettingsController _gameSettingsController = new GameSettingsController();
        [DataMember] bool _MinimizeOnLaunch;
        [DataMember] RecentGameSet _RecentGameSet;
        [DataMember] string _steamDirectory;
        public string SteamDirectory
        {
            get { return _steamDirectory; }
            set
            {
                if (SetProperty(ref _steamDirectory, value))
                    SaveSettings();
            }
        }
        public GameSettingsController GameSettingsController
        {
            get { return _gameSettingsController; }
        }
        public bool CloseOnLaunch
        {
            get { return _CloseOnLaunch; }
            set
            {
                if (SetProperty(ref _CloseOnLaunch, value))
                    SaveSettings();
            }
        }
        public bool MinimizeOnLaunch
        {
            get { return _MinimizeOnLaunch; }
            set
            {
                if (SetProperty(ref _MinimizeOnLaunch, value))
                    SaveSettings();
            }
        }
        Dictionary<string, string> DesiredPackageVersions
        {
            get { return _desiredPackageVersions ?? (_desiredPackageVersions = new Dictionary<string, string>()); }
            set { _desiredPackageVersions = value; }
        }
        List<FavoriteGame> Favorites
        {
            get { return _favorites ?? (_favorites = new List<FavoriteGame>()); }
            set { _favorites = value; }
        }
        public RecentGameSet RecentGameSet
        {
            get { return _RecentGameSet; }
            set { _RecentGameSet = value; }
        }
        List<LegacyGameSettings> GameSettings
        {
            get { return _gameSettings ?? (_gameSettings = new List<LegacyGameSettings>()); }
            set { _gameSettings = value; }
        }
        List<LegacyGameSetSettings> GameSetSettings
        {
            get { return _gameSetSettings ?? (_gameSetSettings = new List<LegacyGameSetSettings>()); }
            set { _gameSetSettings = value; }
        }
        List<FavoriteDlc> FavoriteDlcs
        {
            get { return _favoriteDlcs ?? (_favoriteDlcs = new List<FavoriteDlc>()); }
            set { _favoriteDlcs = value; }
        }
        public GameFilter Filter
        {
            get { return _Filter ?? (_Filter = new GameFilter()); }
            set { _Filter = value; }
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (_gameSettings == null)
                _gameSettings = new List<LegacyGameSettings>();

            if (_gameSetSettings == null)
                _gameSetSettings = new List<LegacyGameSetSettings>();

            if (_gameSettingsController == null)
                _gameSettingsController = new GameSettingsController();
        }

        public string GetDesiredPackageVersion(string packageName) {
            return DesiredPackageVersions.ContainsKey(packageName) ? DesiredPackageVersions[packageName] : null;
        }

        public void SetDesiredPackageVersion(string packageName, string versionData) {
            Contract.Requires<ArgumentOutOfRangeException>(versionData != string.Empty);
            // TODO: What about deeper check; is this really a version denotation, etc..

            if (versionData == null) {
                RemoveDesiredPackageVersion(packageName);
                return;
            }
            if (DesiredPackageVersions.ContainsKey(packageName))
                DesiredPackageVersions[packageName] = versionData;
            else
                DesiredPackageVersions.Add(packageName, versionData);
        }

        void RemoveDesiredPackageVersion(string packageName) {
            if (DesiredPackageVersions.ContainsKey(packageName))
                DesiredPackageVersions.Remove(packageName);
        }

        public bool IsFavorite(Dlc dlc) {
            return FavoriteDlcs.Any(f => f.Matches(dlc));
        }

        public bool IsFavorite(Game game) {
            return Favorites.Any(f => f.Matches(game));
        }

        public void AddFavorite(Game game) {
            if (Favorites.Any(f => f.Matches(game)))
                return;
            Favorites.Add(new FavoriteGame(game));

            SaveSettings();
        }

        public void AddFavorite(Dlc dlc) {
            if (FavoriteDlcs.Any(f => f.Matches(dlc)))
                return;
            FavoriteDlcs.Add(new FavoriteDlc(dlc));

            SaveSettings();
        }

        public void RemoveFavorite(Dlc dlc) {
            var favorite = FavoriteDlcs.FirstOrDefault(f => f.Matches(dlc));
            if (favorite == null)
                return;
            FavoriteDlcs.Remove(favorite);

            SaveSettings();
        }

        public void RemoveFavorite(Game game) {
            var favorite = Favorites.FirstOrDefault(f => f.Matches(game));
            if (favorite == null)
                return;
            Favorites.Remove(favorite);

            SaveSettings();
        }

        public LegacyGameSettings GetLegacyGameSettings(Guid gameId) {
            return GameSettings.FirstOrDefault(x => x.Uuid == gameId);
        }

        [Obsolete("Just for migrating settings, should disappear in a few versions")]
        public void ClearLegacyGameSettings() {
            GameSettings.Clear();
            GameSetSettings.Clear();
        }

        public LegacyGameSetSettings GetLegacyGameSetSettings(Guid gameId) {
            return GameSetSettings.FirstOrDefault(x => x.Uuid == gameId);
        }
    }
}