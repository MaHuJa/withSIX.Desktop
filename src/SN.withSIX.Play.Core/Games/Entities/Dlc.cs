// <copyright company="SIX Networks GmbH" file="Dlc.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public abstract class Dlc : PropertyChangedBase, IHaveId<Guid>
    {
        bool? _isFavorite;

        protected Dlc(Guid id) {
            Contract.Requires<ArgumentOutOfRangeException>(id != Guid.Empty);

            Id = id;
        }

        public abstract DlcMetaData MetaData { get; }
        [Obsolete("Remnant, should we deal differently with this?")]
        public bool IsFavorite
        {
            get
            {
                return _isFavorite == null
                    ? (bool) (_isFavorite = DomainEvilGlobal.Settings.GameOptions.IsFavorite(this))
                    : (bool) _isFavorite;
            }
            set
            {
                if (_isFavorite == value)
                    return;
                if (value)
                    DomainEvilGlobal.Settings.GameOptions.AddFavorite(this);
                else
                    DomainEvilGlobal.Settings.GameOptions.RemoveFavorite(this);
                _isFavorite = value;
                OnPropertyChanged();
            }
        }
        public Guid Id { get; }

        public bool IsInstalled(Game game) {
            var gameInstalledState = game.InstalledState;
            if (!gameInstalledState.IsInstalled)
                return false;

            var dlcDirectory = gameInstalledState.Directory.GetChildDirectoryWithName(MetaData.Name);
            // For now we just need to know if it exists. For the future we might want a DlcInstalledState which contains the folder etc?
            return dlcDirectory.Exists;
        }
    }
}