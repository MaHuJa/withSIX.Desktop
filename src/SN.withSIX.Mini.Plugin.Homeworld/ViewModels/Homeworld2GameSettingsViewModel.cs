// <copyright company="SIX Networks GmbH" file="Homeworld2GameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.Homeworld.ViewModels
{
    public interface IHomeworld2GameSettingsViewModel : IGameSettingsTabViewModel {}

    public class Homeworld2GameSettingsViewModel : GameSettingsTabViewModel, IHomeworld2GameSettingsViewModel
    {
        public override string DisplayName { get; } = "Homeworld 2";
    }
}