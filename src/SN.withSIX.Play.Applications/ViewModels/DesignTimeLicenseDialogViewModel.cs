// <copyright company="SIX Networks GmbH" file="DesignTimeLicenseDialogViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Dialogs;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public class DesignTimeLicenseDialogViewModel : LicenseDialogViewModel, IDesignTimeViewModel
    {
        public DesignTimeLicenseDialogViewModel() {
            var msLicense = new ModSetLicenses("Test Mod") {IsModSetLicensesExpanded = true};
            ModSetLicenses = new List<ModSetLicenses> {msLicense};
            var mod = new Mod(Guid.NewGuid()) {Name = "Test Mod", ModVersion = "1.0.0"};
            var ml = new ModLicense(null, String.Format("{0} {1}", mod.Name, mod.ModVersion)) {
                IsModLicenseExpanded = true
            };
            msLicense.ModLicenses.Add(ml);
        }
    }
}