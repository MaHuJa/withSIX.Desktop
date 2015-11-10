// <copyright company="SIX Networks GmbH" file="GamesViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Usecases;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.Usecases.Main;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.ViewModels.Settings;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games
{
    public class GamesViewModel : ViewModel, IGamesViewModel
    {
        readonly ReactiveCommand<IGameViewModel> _changeGame;
        readonly ReactiveCommand<ISettingsViewModel> _configureGameDirectory;
        readonly IReactiveCommand _openBrowse;
        IGameViewModel _game;

        public GamesViewModel(ISelectionCollectionHelper<IGameItemViewModel> items) {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            Games = items;

            _openBrowse =
                ReactiveCommand.CreateAsyncTask(
                    async x =>
                        await
                            RequestAsync(new OpenWebLink(ViewType.Browse, Games.SelectedItem.Slug))
                                .ConfigureAwait(false))
                    .DefaultSetup("OpenWeb");

            _changeGame = ReactiveCommand.CreateAsyncTask(
                async x => await RequestAsync(new ChangeGame(((IGameItemViewModel) x).Id)).ConfigureAwait(false))
                .DefaultSetup("ChangeGame");
            _changeGame.BindTo(this, x => x.Game);

            _configureGameDirectory =
                ReactiveCommand.CreateAsyncTask(
                    async x =>
                        await
                            OpenScreenCached(new GetSettings {SelectGameTab = true})
                                .ConfigureAwait(false))
                    .DefaultSetup("Configure Game Directory");

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.Games.SelectedItem)
                .Where(x => x != null)
                .ObserveOnMainThread()
                .InvokeCommand(_changeGame));
                d(Listen<ApiGameSelected>()
                    .Select(x => Games.Items.Find(x.Game.Id))
                    .ObserveOnMainThread()
                    .BindTo(this, x => x.Games.SelectedItem));
                d(Listen<GameSettingsUpdated>()
                    .Select(x => new {x, Game = Games.Items.Find(x.Game.Id)})
                    .ObserveOnMainThread()
                    .Subscribe(x => {
                        if (x.x.Game.InstalledState.IsInstalled && x.Game == null) {
                            var item = x.x.Game.MapTo<GameItemViewModel>();
                            Games.Items.Add(item);
                            if (Games.Items.Count == 1)
                                Games.SelectedItem = item;
                        } else if (!x.x.Game.InstalledState.IsInstalled && x.Game != null)
                            Games.Items.Remove(x.Game);
                    }));
            });
        }

        public IGameViewModel Game
        {
            get { return _game; }
            set { this.RaiseAndSetIfChanged(ref _game, value); }
        }
        public ICommand AddGames => _configureGameDirectory;
        public ICommand ConfigureGameDirectory => _configureGameDirectory;
        public ISelectionCollectionHelper<IGameItemViewModel> Games { get; }
        public IReactiveCommand ChangeGame => _changeGame;
        public ICommand OpenBrowse => _openBrowse;
    }

    public interface IGamesViewModel : IViewModel
    {
        ICommand OpenBrowse { get; }
        ISelectionCollectionHelper<IGameItemViewModel> Games { get; }
        IReactiveCommand ChangeGame { get; }
        ICommand AddGames { get; }
        ICommand ConfigureGameDirectory { get; }
        IGameViewModel Game { get; }
    }
}