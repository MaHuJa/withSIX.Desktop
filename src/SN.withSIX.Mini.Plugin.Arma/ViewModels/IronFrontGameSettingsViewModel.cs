// <copyright company="SIX Networks GmbH" file="IronFrontGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface IIronFrontGameSettingsViewModel : IRealVirtualityGameSettingsViewModel {}

    public class IronFrontGameSettingsViewModel : RealVirtualityGameSettingsViewModel, IIronFrontGameSettingsViewModel
    {
        public override string DisplayName { get; } = "Iron Front";
    }
}