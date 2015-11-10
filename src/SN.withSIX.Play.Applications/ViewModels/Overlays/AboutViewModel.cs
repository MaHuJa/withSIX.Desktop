// <copyright company="SIX Networks GmbH" file="AboutViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using ReactiveUI.Legacy;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Play.Applications.ViewModels.Overlays
{
    [DoNotObfuscate]
    public class AboutViewModel : OverlayViewModelBase
    {
        readonly About _about;

        public AboutViewModel() {
            _about = new About();
            DisplayName = "About";

            this.SetCommand(x => x.ApplicationLicensesCommand);
        }

        public string Disclaimer
        {
            get { return _about.Disclaimer; }
        }
        public string Components
        {
            get { return _about.Components; }
        }
        public Version AppVersion
        {
            get { return _about.AppVersion; }
        }
        public string ProductVersion
        {
            get { return _about.ProductVersion; }
        }
        public bool DiagnosticsModeEnabled
        {
            get { return Common.Flags.Verbose; }
        }
        [Browsable(false)]
        public ReactiveCommand ApplicationLicensesCommand { get; private set; }
    }
}