// <copyright company="SIX Networks GmbH" file="RealVirtualityGameSettingsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Applications.ViewModels.Settings;

namespace SN.withSIX.Mini.Plugin.Arma.ViewModels
{
    public interface IRealVirtualityGameSettingsViewModel : IGameSettingsWithConfigurablePackageDirectoryTabViewModel {}

    public abstract class RealVirtualityGameSettingsViewModel : GameSettingsWithConfigurablePackageDirectoryTabViewModel,
        IRealVirtualityGameSettingsViewModel {}
}