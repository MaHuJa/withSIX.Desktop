// <copyright company="SIX Networks GmbH" file="ModSettingsOverlayViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class ModSettingsOverlayViewModel : OverlayViewModelBase, ISingleton
    {
        readonly Lazy<ModsViewModel> _gvm;

        public ModSettingsOverlayViewModel(Lazy<ModsViewModel> gvm) {
            DisplayName = "Mod Settings";
            _gvm = gvm;
        }

        public ModsViewModel GVM
        {
            get { return _gvm.Value; }
        }

        [DoNotObfuscate]
        public void SaveUserconfig() {
            var item = GVM.LibraryVM.SelectedItem.SelectedItem;
            var mod = item.ToMod();
            mod.UserConfig.Save();
        }
    }
}