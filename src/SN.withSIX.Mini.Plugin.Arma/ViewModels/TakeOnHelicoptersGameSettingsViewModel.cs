// <copyright company="SIX Networks GmbH" file="TakeOnHelicoptersGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface ITakeOnHelicoptersGameSettingsViewModel : IRealVirtualityGameSettingsViewModel {}

    public class TakeOnHelicoptersGameSettingsViewModel : RealVirtualityGameSettingsViewModel,
        ITakeOnHelicoptersGameSettingsViewModel
    {
        public override string DisplayName { get; } = "Take on Helicopters";
    }
}