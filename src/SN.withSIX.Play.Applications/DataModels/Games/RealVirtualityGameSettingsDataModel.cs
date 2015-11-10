// <copyright company="SIX Networks GmbH" file="RealVirtualityGameSettingsDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using SN.withSIX.Play.Core.Games.Legacy.Arma;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class RealVirtualityGameSettingsDataModel : GameSettingsDataModel
    {
        bool _resetGameKeyEachLaunch;
        protected bool _useSteamLauncher;
        [DisplayName("(Legacy) Launch with Steam Launcher")]
        [Category(GameSettingCategories.Launching)]
        [Description("If regularly launching a Steam game does not work use this option.")]
        public bool LaunchUsingSteam
        {
            get { return _useSteamLauncher; }
            set { SetProperty(ref _useSteamLauncher, value); }
        }
        [DisplayName("Reset Game Key on launch")]
        [Category(GameSettingCategories.Launching)]
        [Description("Reset Game Key via Steam each launch")]
        public bool ResetGameKeyEachLaunch
        {
            get { return _resetGameKeyEachLaunch; }
            set { SetProperty(ref _resetGameKeyEachLaunch, value); }
        }
    }
}