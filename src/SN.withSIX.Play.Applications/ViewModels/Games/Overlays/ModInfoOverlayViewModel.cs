// <copyright company="SIX Networks GmbH" file="ModInfoOverlayViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class ModInfoOverlayViewModel : OverlayViewModelBase, ISingleton
    {
        readonly Lazy<ModsViewModel> _mvm;
        bool _isEditingGlobalVersion;
        [DoNotObfuscate] ObservableAsPropertyHelper<bool> _isInCollection;
        [DoNotObfuscate] ObservableAsPropertyHelper<IMod> _selectedItem;

        public ModInfoOverlayViewModel(Lazy<ModsViewModel> mvm) {
            DisplayName = "Mod Info";
            _mvm = mvm;
        }

        public IMod SelectedItem
        {
            get { return _selectedItem.Value; }
        }
        public bool IsInCollection
        {
            get { return _isInCollection.Value; }
        }
        public bool IsEditingGlobalVersion
        {
            get { return _isEditingGlobalVersion; }
            set { SetProperty(ref _isEditingGlobalVersion, value); }
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            _selectedItem = _mvm.Value.LibraryVM.WhenAnyValue(x => x.SelectedItem.SelectedItem)
                .OfType<IMod>()
                .ToProperty(this, x => x.SelectedItem);

            _isInCollection = this.WhenAny(x => x.SelectedItem, x => x.Value is ToggleableModProxy)
                .ToProperty(this, x => x.IsInCollection);
        }

        [DoNotObfuscate]
        public void ShowInfo() {
            BrowserHelper.TryOpenUrlIntegrated(SelectedItem.ProfileUrl());
        }

        [DoNotObfuscate]
        public void ShowHomepage() {
            _mvm.Value.ViewHomepageAction(SelectedItem);
        }

        [DoNotObfuscate]
        public void EditGlobalVersion() {
            IsEditingGlobalVersion = true;
            //var mod = SelectedItem;
            //GVM.LibraryVM.SelectNetwork();
            //GVM.LibraryVM.SelectedItem.SelectedItem = mod.ToMod();
        }

        [DoNotObfuscate]
        public void CloseEditGlobalVersion() {
            IsEditingGlobalVersion = false;
        }

        [DoNotObfuscate]
        public void ShowChangelog() {
            _mvm.Value.ViewChangelogAction(SelectedItem);
        }
    }
}