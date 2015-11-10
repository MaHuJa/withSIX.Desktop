// <copyright company="SIX Networks GmbH" file="IDashboardViewModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using Caliburn.Micro;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DoNotObfuscate]
    public interface IDashboardViewModelBase : INotifyPropertyChanged, IScreen
    {
        void ShowAbout();
        void ShowSettings();
        void ShowLicenses();
        void ShowCheckForUpdates();
    }
}