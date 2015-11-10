// <copyright company="SIX Networks GmbH" file="DesignTimePlayShellViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Connect;
using SN.withSIX.Play.Applications.ViewModels.Games;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public class DesignTimePlayShellViewModel : PlayShellViewModel, IDesignTimeViewModel
    {
        public DesignTimePlayShellViewModel() {
            DisplayName = "Play withSIX - DEV";
            Home = new DesignTimeHomeViewModel();
            Connect = new DesignTimeConnectViewModel();
            //Games = new DesignTimeGamesViewModel();
            var designTimeServersViewModel = new DesignTimeServersViewModel();
            /*            ActiveGame =
                new GameViewModel(new Arma2Game(Guid.NewGuid(), SN.withSIX.Play.Core.Options.UserSettings.Current.GameOptions.GameSettingsController),
                    designTimeServersViewModel, new DesignTimeModsViewModel(), new DesignTimeMissionsViewModel());*/

            Connect.IsEnabled = true;
            //ActiveItem = designTimeServersViewModel;

            ContactListWidth = 244;
            MinWidth = 1024;
            Width = 1024;

            //ExternalApps.Add(new ExternalApp("test app", "C:\\test path", "testpars", false, StartupType.Any));

            /*ActiveGame.SelectedModule = true;*/

            /*
            if (Servers.IsActive)
                ShowServerList();
            else if (Missions.IsActive)
                ShowMissionList();
            */
        }
    }
}