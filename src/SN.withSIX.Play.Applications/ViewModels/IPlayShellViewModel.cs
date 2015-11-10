// <copyright company="SIX Networks GmbH" file="IPlayShellViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels
{
    [DoNotObfuscate]
    public interface IPlayShellViewModel : IShellViewModel, IShellViewModelTrayIcon, IScreen, IHaveOverlayConductor,
        ISupportsActivation
        //, IConductor
    {
        OverlayViewModelBase SubOverlay { get; }
        IContentViewModel Content { get; }
        SettingsViewModel Settings { get; }
        IStatusViewModel StatusFlyout { get; }
        IViewModelFactory Factory { get; }
        bool? GridMode { get; }
        IObservable<bool> ActivateWindows { get; }
        ISoftwareUpdate SoftwareUpdate { get; }
        ReactiveCommand Exit { get; }
        //string Icon { get; }
        void ShowOverlay(OverlayViewModelBase overlay);
        void CloseOverlay();
    }
}