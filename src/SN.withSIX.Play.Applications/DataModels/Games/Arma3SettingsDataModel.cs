// <copyright company="SIX Networks GmbH" file="Arma3SettingsDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using SN.withSIX.Play.Core.Games.Legacy.Arma;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public class Arma3SettingsDataModel : Arma2OaSettingsDataModel
    {
        [DisplayName("(Legacy) Launch with Steam Launcher")]
        [Category(GameSettingCategories.Launching)]
        [Description("If regularly launching a Steam game does not work use this option.")]
        [Browsable(false)]
        public new bool LaunchUsingSteam
        {
            get { return _useSteamLauncher; }
            set { SetProperty(ref _useSteamLauncher, value); }
        }
        [Category(GameSettingCategories.Launching)]
        [DisplayName("Use Steam In-Game")]
        [Description("Add the Steam In-Game Overlay to your game (Even if it does not support Steam!)")]
        [Browsable(false)]
        public bool InjectSteam
        {
            get { return _injectSteam; }
            set { SetProperty(ref _injectSteam, value); }
        }
    }
}