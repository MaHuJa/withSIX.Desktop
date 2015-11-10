// <copyright company="SIX Networks GmbH" file="OptionsMenu.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;

namespace SN.withSIX.Play.Applications.ViewModels.Popups
{
    public class OptionsMenu : PopupMenuBase
    {
        readonly OptionsMenuViewModel _optionsMenuViewModelViewModel;

        public OptionsMenu(OptionsMenuViewModel optionsMenuViewModel) {
            _optionsMenuViewModelViewModel = optionsMenuViewModel;
            DisplayName = "Options";
        }

        protected override bool UppercaseFirst
        {
            get { return false; }
        }

        [DoNotObfuscate, MenuItem(icon: SixIconFont.withSIX_icon_Hexagon_Help)]
        public void About() {
            _optionsMenuViewModelViewModel.SwitchAbout();
        }

        [DoNotObfuscate, MenuItem(icon: SixIconFont.withSIX_icon_Apps)]
        public void Apps() {
            _optionsMenuViewModelViewModel.SwitchApps();
        }

        [DoNotObfuscate, MenuItem(icon: SixIconFont.withSIX_icon_Licenses)]
        public void Licenses() {
            _optionsMenuViewModelViewModel.OpenLicenses();
        }

        [DoNotObfuscate, MenuItem(icon: SixIconFont.withSIX_icon_Hexagon_SelfUpdating)]
        public void CheckForUpdates() {
            _optionsMenuViewModelViewModel.ShowCheckForUpdates();
        }

        [DoNotObfuscate, MenuItem(icon: SixIconFont.withSIX_icon_Settings)]
        public void Settings() {
            _optionsMenuViewModelViewModel.OpenSettings();
        }
    }
}