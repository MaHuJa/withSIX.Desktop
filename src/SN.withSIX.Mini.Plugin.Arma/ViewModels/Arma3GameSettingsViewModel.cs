// <copyright company="SIX Networks GmbH" file="Arma3GameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using ReactiveUI;

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface IArma3GameSettingsViewModel : IRealVirtualityGameSettingsViewModel
    {
        bool LaunchThroughBattlEye { get; set; }
    }

    public class Arma3GameSettingsViewModel : RealVirtualityGameSettingsViewModel, IArma3GameSettingsViewModel
    {
        bool _launchThroughBattlEye;
        public override string DisplayName { get; } = "Arma 3";
        public bool LaunchThroughBattlEye
        {
            get { return _launchThroughBattlEye; }
            set { this.RaiseAndSetIfChanged(ref _launchThroughBattlEye, value); }
        }
    }
}