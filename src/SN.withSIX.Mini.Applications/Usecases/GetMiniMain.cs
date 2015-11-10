// <copyright company="SIX Networks GmbH" file="GetMiniMain.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Mini.Applications.Core;
using SN.withSIX.Mini.Applications.Extensions;
using SN.withSIX.Mini.Applications.Models;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Applications.ViewModels;
using SN.withSIX.Mini.Applications.ViewModels.Main;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Applications.ViewModels.Main.Welcome;
using Splat;
using Squirrel;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public interface IPlaySquirrel
    {
        Task<Version> GetNewVersion();
    }

    public interface ISquirrelUpdater
    {
        Task<UpdateInfo> CheckForUpdates();
        Task<ReleaseEntry> UpdateApp(Action<int> progressAction);
    }

    public class GetMiniMain : IAsyncQuery<IMiniMainWindowViewModel> {}

    public class GetMiniMainHandler : DbQueryBase, IAsyncRequestHandler<GetMiniMain, IMiniMainWindowViewModel>
    {
        readonly IPlaySquirrel _squirrel;
        readonly IStateHandler _stateHandler;

        public GetMiniMainHandler(IDbContextLocator dbContextLocator,
            IPlaySquirrel squirrel, IStateHandler stateHandler)
            : base(dbContextLocator) {
            _squirrel = squirrel;
            _stateHandler = stateHandler;
        }

        public async Task<IMiniMainWindowViewModel> HandleAsync(GetMiniMain request) {
            var trayMainWindowViewModel = await GetTrayViewModel().ConfigureAwait(false);
            var miniMainWindowViewModel = new MiniMainWindowViewModel(trayMainWindowViewModel);
            HandleUpdateState(trayMainWindowViewModel);
            return miniMainWindowViewModel;
        }

        // TODO: Move to some initialization that happens after the mainwindow?
        void HandleUpdateState(TrayMainWindowViewModel trayMainWindowViewModel) {
            if (Consts.NewVersionInstalled)
                trayMainWindowViewModel.UpdateState = AppUpdateState.UpdateInstalled;
            Task.Run(async () => {
                await OnElapsed().ConfigureAwait(false);
                var timer = new TimerWithElapsedCancellationAsync(TimeSpan.FromHours(1).TotalMilliseconds, OnElapsed);
            });
        }

        async Task<bool> OnElapsed() {
            try {
                Consts.NewVersionAvailable = await _squirrel.GetNewVersion().ConfigureAwait(false);
                if (Consts.NewVersionAvailable != null) {
                    await new AppStateUpdated(AppUpdateState.UpdateAvailable).RaiseEvent().ConfigureAwait(false);
                    return false;
                }
            } catch (Exception ex) {
                MainLog.Logger.Write(ex.Format(), LogLevel.Warn);
            }
            return true;
        }

        async Task<TrayMainWindowViewModel> GetTrayViewModel() {
            var loginInfo = SettingsContext.Settings.Secure.Login ?? LoginInfo.Default;
            var mainArea = new GamesViewModel(await GetGames().ConfigureAwait(false));
            return new TrayMainWindowViewModel(
                mainArea,
                new TrayMainWindowMenu(loginInfo),
                new StatusViewModel(_stateHandler.StatusObservable), loginInfo, new WelcomeViewModel());
            // TODO: Get StatusModel of current game...
        }

        async Task<SelectionCollectionHelper<IGameItemViewModel>> GetGames() {
            var selectedGameId = SettingsContext.Settings.Local.SelectedGameId;
            await GameContext.LoadAll().ConfigureAwait(false);
            var items = await GameContext.Games
                .Where(x => (Consts.Features.UnreleasedGames || x.Metadata.IsPublic) && x.InstalledState.IsInstalled)
                .OrderByDescending(x => x.LastPlayed)
                .Select(x => x.MapTo<GameItemViewModel>())
                .ToListAsync()
                .ConfigureAwait(false);
            return new SelectionCollectionHelper<IGameItemViewModel>(items,
                items.FirstOrDefault(x => x.Id == selectedGameId));
        }
    }

    public class AppStateUpdated : IDomainEvent
    {
        public AppStateUpdated(AppUpdateState updateState) {
            UpdateState = updateState;
        }

        public AppUpdateState UpdateState { get; }
    }

    public enum AppUpdateState
    {
        Uptodate,
        UpdateInstalled,
        UpdateAvailable,
        Updating
    }
}