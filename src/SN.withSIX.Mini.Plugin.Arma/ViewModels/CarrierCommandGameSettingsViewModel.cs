// <copyright company="SIX Networks GmbH" file="CarrierCommandGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface ICarrierCommandGameSettingsViewModel : IGameSettingsTabViewModel {}

    public class CarrierCommandGameSettingsViewModel : GameSettingsTabViewModel, ICarrierCommandGameSettingsViewModel
    {
        public override string DisplayName { get; } = "Carrier Command";
    }
}