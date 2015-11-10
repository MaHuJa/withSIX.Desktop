// <copyright company="SIX Networks GmbH" file="EvilGlobalSelectedGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Core.Options.Entries;
using PropertyChangedBase = SN.withSIX.Core.Helpers.PropertyChangedBase;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [Obsolete("Destroy")]
    public class EvilGlobalSelectedGame : PropertyChangedBase, IDomainService
    {
        readonly UserSettings _settings;
        Game _activeGame;
        bool _first;

        public EvilGlobalSelectedGame(UserSettings settings) {
            _settings = settings;
        }

        public Game ActiveGame
        {
            get { return _activeGame; }
            set
            {
                if (_activeGame == value)
                    return;
                _activeGame = value;

                if (!Execute.InDesignMode && value != null)
                    UsageCounter.ReportUsage("Active game: {0}".FormatWith(value.MetaData.Name));

                if (_first)
                    HandleGameSetChanged(value);
                else {
                    _first = true;
                    OnPropertyChanged();
                }
            }
        }

        async void HandleGameSetChanged(Game x) {
            _settings.Ready = false;
            _settings.GameOptions.RecentGameSet = x == null ? null : new RecentGameSet(x);
            OnPropertyChanged("ActiveGame");
            await
                Task.Factory.StartNew(() => TryActiveGameChanged(x), TaskCreationOptions.LongRunning).Unwrap()
                    .ConfigureAwait(false);
        }

        async Task TryActiveGameChanged(Game x) {
            try {
                x.RefreshState();
                await Common.App.Mediator.NotifyEnMass(new ActiveGameChanged(x)).ConfigureAwait(false);
                await Common.App.Mediator.NotifyEnMass(new ActiveGameChangedForReal(x)).ConfigureAwait(false);
            } finally {
                _settings.Ready = true;
            }
        }
    }

    public class ActiveGameChangedForReal
    {
        public ActiveGameChangedForReal(Game game) {
            Game = game;
        }

        public Game Game { get; private set; }
    }
}