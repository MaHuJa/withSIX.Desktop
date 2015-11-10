// <copyright company="SIX Networks GmbH" file="DayZGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface IDayZGameSettingsViewModel : IRealVirtualityGameSettingsViewModel {}

    public class DayZGameSettingsViewModel : RealVirtualityGameSettingsViewModel, IDayZGameSettingsViewModel
    {
        public override string DisplayName { get; } = "DayZ";
    }
}