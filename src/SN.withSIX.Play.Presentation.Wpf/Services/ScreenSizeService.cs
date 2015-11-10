// <copyright company="SIX Networks GmbH" file="ScreenSizeService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Games.Services.GameLauncher;

namespace SN.withSIX.Play.Presentation.Wpf.Services
{
    public class ScreenSizeService : IGetScreenSize, IApplicationService
    {
        public ScreenResolution ScreenSize() {
            return new ScreenResolution(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }
    }
}