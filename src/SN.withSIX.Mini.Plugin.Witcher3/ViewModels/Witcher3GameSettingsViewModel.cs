// <copyright company="SIX Networks GmbH" file="Witcher3GameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.Witcher3.ViewModels
{
    public interface IWitcher3GameSettingsViewModel : IGameSettingsTabViewModel {}

    public class Witcher3GameSettingsViewModel : GameSettingsTabViewModel, IWitcher3GameSettingsViewModel
    {
        public override string DisplayName { get; } = "Witcher 3";
    }
}