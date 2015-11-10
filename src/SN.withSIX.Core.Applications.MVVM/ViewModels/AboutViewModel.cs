// <copyright company="SIX Networks GmbH" file="AboutViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI.Legacy;
using SN.withSIX.Core.Applications.Services;
using Six.Core;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DoNotObfuscate]
    public class AboutViewModel : ReactiveModalScreen<IShellViewModelFullBase>
    {
        public AboutViewModel(About about) {
            About = about;
            base.DisplayName = "about " + Common.App.AppTitle;

            this.SetCommand(x => x.ApplicationLicensesCommand);
            ApplicationLicensesCommand.Subscribe(x => ShowLicenses());
        }

        public ReactiveCommand ApplicationLicensesCommand { get; protected set; }
        public About About { get; set; }

        [DoNotObfuscate]
        public void ShowLicenses() {
            ParentShell.ShowLicenses();
        }
    }
}