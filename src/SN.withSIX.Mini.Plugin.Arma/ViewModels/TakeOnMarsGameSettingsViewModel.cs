// <copyright company="SIX Networks GmbH" file="TakeOnMarsGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface ITakeOnMarsGameSettingsViewModel : IGameSettingsTabViewModel {}

    public class TakeOnMarsGameSettingsViewModel : GameSettingsTabViewModel, ITakeOnMarsGameSettingsViewModel
    {
        public override string DisplayName { get; } = "Take On Mars";
    }
}