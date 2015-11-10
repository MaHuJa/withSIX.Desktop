// <copyright company="SIX Networks GmbH" file="DesignTimeServersViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Caliburn.Micro;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class DesignTimeServersViewModel : ServersViewModel, IDesignTimeViewModel
    {
        public DesignTimeServersViewModel() {
            Settings = IoC.Get<SettingsViewModel>();
            ServerFilter = new ArmaServerFilter();
        }
    }
}