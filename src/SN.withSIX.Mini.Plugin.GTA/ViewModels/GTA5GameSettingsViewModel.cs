// <copyright company="SIX Networks GmbH" file="GTA5GameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.GTA.ViewModels
{
    public interface IGTA5GameSettingsViewModel : IGameSettingsTabViewModel {}

    public class GTA5GameSettingsViewModel : GameSettingsTabViewModel, IGTA5GameSettingsViewModel
    {
        public override string DisplayName { get; } = "GTA 5";
    }
}