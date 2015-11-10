// <copyright company="SIX Networks GmbH" file="IGameViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows.Input;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games.Recent;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games
{
    public class GameViewModel : ViewModel, IGameViewModel
    {
        readonly ReactiveCommand<UnitType> _firstTimeClose;
        bool _firstTimeRunShown;

        public GameViewModel(Guid id, ISelectionCollectionHelper<IGameTabViewModel> tabs) {
            Id = id;
            Tabs = tabs;

            _firstTimeClose =
                ReactiveCommand.CreateAsyncTask(
                    async x => await RequestAsync(new CloseFirstTimeInfo(Id)).ConfigureAwait(false))
                    .DefaultSetup("Close");
            _firstTimeClose.Subscribe(x => FirstTimeRunShown = true);

            // TODO: Consider to separate tab viewmodels from actual viewmodels so we can load the content lazily
            // or make the tabs load their content lazily on first use etc?
        }

        public ISelectionCollectionHelper<IGameTabViewModel> Tabs { get; }
        public string FirstTimeRunInfo { get; protected set; }
        public bool FirstTimeRunShown
        {
            get { return _firstTimeRunShown; }
            protected set { this.RaiseAndSetIfChanged(ref _firstTimeRunShown, value); }
        }
        public ICommand FirstTimeClose => _firstTimeClose;
        public Guid Id { get; }
    }

    public interface IGameViewModel : IHaveId<Guid>
    {
        ISelectionCollectionHelper<IGameTabViewModel> Tabs { get; }
        string FirstTimeRunInfo { get; }
        bool FirstTimeRunShown { get; }
        ICommand FirstTimeClose { get; }
    }
}