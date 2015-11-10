// <copyright company="SIX Networks GmbH" file="GamesSettingsTabViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Applications.Annotations;
using SN.withSIX.Mini.Applications.Usecases.Settings;

namespace SN.withSIX.Mini.Applications.ViewModels.Settings
{
    public interface IGamesSettingsTabViewModel : ISettingsTabViewModel
    {
        IReactiveList<DetectedGameItemViewModel> DetectedGames { get; }
        IDetectedGameItemViewModel SelectedGame { get; set; }
        IGameSettingsTabViewModel Game { get; set; }
        ICommand ChangeGame { get; }
    }

    [Order(3)]
    public class GamesSettingsTabViewModel : SettingsTabViewModel, IGamesSettingsTabViewModel
    {
        readonly ReactiveCommand<IGameSettingsTabViewModel> _changeGame;
        readonly IList<IGameSettingsTabViewModel> _subItems = new List<IGameSettingsTabViewModel>();
        IGameSettingsTabViewModel _game;
        IDetectedGameItemViewModel _selectedGame;

        public GamesSettingsTabViewModel(Guid selectedGameId, IEnumerable<DetectedGameItemViewModel> detectedGames) {
            DetectedGames = new ReactiveList<DetectedGameItemViewModel>(detectedGames);
            _selectedGame = DetectedGames.Find(selectedGameId) ?? DetectedGames.First();

            _changeGame = ReactiveCommand.CreateAsyncTask(async x => {
                var selectedGame = SelectedGame;
                if (selectedGame == null) {
                    Game = null;
                    return null;
                }
                var existing = _subItems.Find(selectedGame.Id);
                if (existing != null) {
                    Game = existing;
                    return existing;
                }
                var item = await RequestAsync(new GetGameSettings(selectedGame.Id)).ConfigureAwait(false);
                _subItems.Add(item);
                return item;
            }).DefaultSetup("SelectGame");
            _changeGame.BindTo(this, x => x.Game);

            this.WhenAnyValue(x => x.SelectedGame)
                .ObserveOnMainThread()
                .InvokeCommand(_changeGame);
        }

        public override string DisplayName { get; } = "Games";
        public IGameSettingsTabViewModel Game
        {
            get { return _game; }
            set { this.RaiseAndSetIfChanged(ref _game, value); }
        }
        public IReactiveList<DetectedGameItemViewModel> DetectedGames { get; }
        public IDetectedGameItemViewModel SelectedGame
        {
            get { return _selectedGame; }
            set { this.RaiseAndSetIfChanged(ref _selectedGame, value); }
        }
        public override IEnumerable<ISettingsTabViewModel> SubItems => _subItems;
        public ICommand ChangeGame => _changeGame;
    }

    public interface IDetectedGameItemViewModel : IHaveId<Guid>
    {
        string Name { get; }
    }

    public class DetectedGameItemViewModel : IDetectedGameItemViewModel
    {
        public DetectedGameItemViewModel(Guid id, string metadataName) {
            Id = id;
            Name = metadataName;
        }

        public string Name { get; }
        public Guid Id { get; }
    }
}