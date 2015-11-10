// <copyright company="SIX Networks GmbH" file="GTA4GameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.GTA.ViewModels
{
    public interface IGTA4GameSettingsViewModel : IGameSettingsTabViewModel {}

    public class GTA4GameSettingsViewModel : GameSettingsTabViewModel, IGTA4GameSettingsViewModel
    {
        public override string DisplayName { get; } = "GTA 4";
    }
}