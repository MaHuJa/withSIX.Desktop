// <copyright company="SIX Networks GmbH" file="RealVirtualityGameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    public abstract class RealVirtualityGameSettings : GameSettingsWithConfigurablePackageDirectory
    {
        protected string[] DefaultStartupParameters = {"-nosplash", "-nofilepatching"};
    }
}