// <copyright company="SIX Networks GmbH" file="GameViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI.Legacy;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class GameViewModel : ViewModelBase
    {
        readonly Lazy<ContentViewModel> _cvmLazy;
        readonly Game _game;
        // TODO: We probably want to create a new viewmodel for each game, also pass in the game?
        // Or should we merge these viewmodels into this one as the more important part is the libraries that are within those viewmodels?
        public GameViewModel(Game game, ServersViewModel serversViewModel, ModsViewModel modsViewModel,
            MissionsViewModel missionsViewModel, Lazy<ContentViewModel> cvmLazy) {
            _game = game;
            _cvmLazy = cvmLazy;
            if (game.SupportsServers())
                Servers = serversViewModel;
            serversViewModel.SwitchGame(game);

            if (game.SupportsMods())
                Mods = modsViewModel;
            modsViewModel.SwitchGame(game);

            if (game.SupportsMissions())
                Missions = missionsViewModel;
            missionsViewModel.SwitchGame(game);

            this.SetCommand(x => ActivateServerList).Subscribe(() => ShowServerList());
            this.SetCommand(x => ActivateMissionList).Subscribe(() => ShowMissionList());
        }

        public ReactiveCommand ActivateServerList { get; private set; }
        public ReactiveCommand ActivateMissionList { get; private set; }
        public MissionsViewModel Missions { get; }
        public ModsViewModel Mods { get; private set; }
        public ServersViewModel Servers { get; }
        public bool SelectedModule
        {
            get { return DomainEvilGlobal.Settings.AppOptions.SelectedModule; }
            set
            {
                DomainEvilGlobal.Settings.AppOptions.SelectedModule = value;
                OnPropertyChanged();
            }
        }

        void ShowServerList(bool sw = true) {
            if (sw && !SelectedModule)
                _game.CalculatedSettings.SwitchToServer();
            SelectedModule = true;
            _cvmLazy.Value.ActivateItem(Servers);
        }

        public void ShowMissionList(bool sw = true) {
            if (sw && SelectedModule)
                _game.CalculatedSettings.SwitchToMission();

            SelectedModule = false;
            _cvmLazy.Value.ActivateItem(Missions);
        }

        public void InitModules() {
            if (_game.SupportsServers() && Servers.IsActive)
                ShowServerList(false);
            else if (_game.SupportsMissions() && Missions.IsActive)
                ShowMissionList(false);
        }
    }
}