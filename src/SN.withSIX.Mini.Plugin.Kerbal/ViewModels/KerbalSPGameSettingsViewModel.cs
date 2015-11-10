// <copyright company="SIX Networks GmbH" file="KerbalSPGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.Kerbal.ViewModels
{
    public interface IKerbalSPGameSettingsViewModel : IGameSettingsTabViewModel {}

    public class KerbalSPGameSettingsViewModel : GameSettingsTabViewModel, IKerbalSPGameSettingsViewModel
    {
        public override string DisplayName { get; } = "Kerbal Space Program";
    }
}