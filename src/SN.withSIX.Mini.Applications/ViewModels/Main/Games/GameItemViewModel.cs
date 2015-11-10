// <copyright company="SIX Networks GmbH" file="GameItemViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games
{
    public static class ViewModelExtensions
    {
        public static IObservable<bool> GetGameLockObservable(this ViewModel vmThis, Guid gameId) {
            return Cheat.GameLockMonitor.GetObservable(gameId).Select(x => !x).ObserveOnMainThread();
        }
    }
    public class GameItemViewModel : ViewModel, IGameItemViewModel
    {
        bool _isInstalled;
        readonly IReactiveCommand _launch;

        public GameItemViewModel(Guid id, ISelectionCollectionHelper<LaunchType> launchTypes) {
            Id = id;
            LaunchTypes = launchTypes;
            _launch = ReactiveCommand.CreateAsyncTask(this.GetGameLockObservable(id), async x =>
                await
                    RequestAsync(new LaunchGame(Id, LaunchTypes.SelectedItem))
                        .ConfigureAwait(false))
                .DefaultSetup("LaunchGame");

            LaunchTypes.WhenAnyValue(x => x.SelectedItem)
                .Skip(1)
                .InvokeCommand(Launch);

            Listen<GameSettingsUpdated>()
                .Where(x => Id == x.Game.Id)
                .Select(x => x.Game.InstalledState.IsInstalled)
                .ObserveOnMainThread()
                .BindTo(this, x => x.IsInstalled);

            // TODO: These only activate when opening the combobox, not when the game is currently selected in the combobox :(
            /*this.WhenActivated(d => {
            });*/
        }

        public string Name { get; protected set; }
        public Uri Image { get; protected set; }
        public Uri BackgroundImage { get; protected set; }
        public string Slug { get; protected set; }
        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { this.RaiseAndSetIfChanged(ref _isInstalled, value); }
        }
        public ISelectionCollectionHelper<LaunchType> LaunchTypes { get; }
        public ICommand Launch => _launch;
        public Guid Id { get; }
    }

    public interface IGameItemViewModel : IViewModel, IHaveId<Guid>
    {
        string Name { get; }
        Uri Image { get; }
        Uri BackgroundImage { get; }
        string Slug { get; }
        bool IsInstalled { get; set; }
        ISelectionCollectionHelper<LaunchType> LaunchTypes { get; }
        ICommand Launch { get; }
    }
}