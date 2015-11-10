// <copyright company="SIX Networks GmbH" file="IShellViewModelFullBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DoNotObfuscate]
    public interface IShellViewModelFullBase : IShellViewModelBase
    {
        void ShowAbout();
        void ShowLicenses();
        void ShowOptions();
        //void ShowPrevious();
        void UpdateSoftware(bool b);
    }
}