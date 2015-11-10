// <copyright company="SIX Networks GmbH" file="ModsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using NDepend.Path;
using ReactiveUI;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Applications.ViewModels.Games.Popups;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    [DoNotObfuscate]
    public class ModsViewModel : LibraryModuleViewModel, IHandle<SubGamesChanged>,
        IHandle<GameContentInitialSynced>, IHandle<ActiveGameChangedForReal>
    {
        readonly Lazy<IContentManager> _contentManager;
        readonly IDialogManager _dialogManager;
        readonly IViewModelFactory _factory;
        readonly object _libLock = new object();
        readonly Lazy<IUpdateManager> _updateManager;
        ModLibraryViewModel _libraryVm;
        protected ModsViewModel() {}

        public ModsViewModel(ModSettingsOverlayViewModel msovm,
            ModInfoOverlayViewModel miovm, ModVersionOverlayViewModel mvovm, IDialogManager dialogManager,
            Lazy<IUpdateManager> updateManager, IViewModelFactory factory,
            Lazy<IContentManager> contentManager, UserSettings settings) {
            _updateManager = updateManager;
            _contentManager = contentManager;
            _dialogManager = dialogManager;
            _factory = factory;
            UserSettings = settings;

            ModuleName = ControllerModules.ModBrowser;
            DisplayName = "Mods";
            ModSetSettingsOverlay = msovm;
            ModSetInfoOverlay = miovm;
            ModSetVersionOverlay = mvovm;
        }

        public UserSettings UserSettings { get; }
        public ModLibraryViewModel LibraryVM
        {
            get { return _libraryVm; }
            protected set { SetProperty(ref _libraryVm, value); }
        }
        public ModSettingsOverlayViewModel ModSetSettingsOverlay { get; protected set; }
        public ModInfoOverlayViewModel ModSetInfoOverlay { get; protected set; }
        public ModInfoOverlayViewModel ModSetVersionOverlay { get; protected set; }
        public bool LockDown
        {
            get { return Common.Flags.LockDown; }
        }
        public IContentManager ContentManager
        {
            get { return _contentManager.Value; }
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            if (!Execute.InDesignMode) {
                this.WhenAnyValue(x => x.IsActive)
                    .Skip(1).Subscribe(value => UsageCounter.ReportUsage("Tab Mods"));
            }

            this.WhenAnyValue(x => x.ModSetSettingsOverlay.IsActive)
                .Where(x => x).Subscribe(x => ModSetInfoOverlay.TryClose());

            this.WhenAnyValue(x => x.ModSetInfoOverlay.IsActive)
                .Where(x => x).Subscribe(x => ModSetSettingsOverlay.TryClose());

            this.WhenAnyValue(x => x.LibraryVM.IsLoading).Subscribe(x => ProgressState.Active = x);

            this.WhenAnyValue(x => x.LibraryVM.SelectedItem.SelectedItem)
                .OfType<IMod>()
                .Where(x => x != null)
                .Select(x => new {x, Game = DomainEvilGlobal.SelectedGame.ActiveGame as ISupportModding})
                .Where(x => x.Game != null)
                .Subscribe(x => x.x.LoadSettings(x.Game));

            DomainEvilGlobal.SelectedGame.WhenAnyValue(x => x.ActiveGame)
                .Subscribe(ReplaceLibraryIfRequired);
        }

        void ReplaceLibraryIfRequired(Game x) {
            lock (_libLock) {
                if (LibraryVM == null || LibraryVM.Game != x)
                    CreateLibrary(x);
            }
        }

        public IMod GetSelectedMod() {
            var modLib = LibraryVM;
            if (modLib == null)
                return null;
            var selected = modLib.SelectedItem;
            if (selected == null)
                return null;
            var content = selected.SelectedItem;
            return content != null ? content.ToMod() : null;
        }

        void CreateLibrary(Game game) {
            if (game.SupportsMods()) {
                UiHelper.TryOnUiThread(
                    () => {
                        var modLibraryViewModel = _factory.CreateModLibraryViewModel(game).Value;
                        ((IActivate) modLibraryViewModel).Activate();
                        LibraryVM = modLibraryViewModel;
                    });
            } else
                LibraryVM = null;
        }

        public Task<CustomCollection> CreateModSet(IContent content = null) {
            return Task.Run(() => _contentManager.Value.CreateAndSelectCustomModSet(content));
        }

        public Task<CustomCollection> CreateModSet(IReadOnlyCollection<IContent> content) {
            return Task.Run(() => _contentManager.Value.CreateAndSelectCustomModSet(content));
        }

        public Task ShowAddRepository() {
            return _dialogManager.ShowPopupAsync(_factory.CreateAddRepository().Value);
        }

        public Task ShowCreateCollection() {
            return _dialogManager.ShowPopupAsync(new CreateCollectionViewModel(_contentManager.Value));
        }

        [DoNotObfuscate, SmartAssembly.Attributes.ReportUsage]
        public void SelectNoModSet() {
            LibraryVM.ActiveItem = null;
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ModSetConfigure(object x) {
            ShowOverlay(ModSetSettingsOverlay);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ModSetInfo() {
            ShowOverlay(ModSetInfoOverlay);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ModVersion() {
            ShowOverlay(ModSetVersionOverlay);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async Task ModUninstall(IMod mod) {
            Contract.Requires<ArgumentNullException>(mod != null);

            LibraryVM.ActiveItem = mod;

            var report =
                !_dialogManager.MessageBoxSync(
                    new MessageBoxDialogParams(string.Format("You are about to uninstall {0}\nare you sure?",
                        mod.Name), "Are you sure you want to uninstall the Modification?", SixMessageBoxButton.YesNo) {
                            IgnoreContent = false,
                            GreenContent = "delete",
                            RedContent = "keep"
                        }).IsYes();

            UsageCounter.ReportUsage("Dialog - Uninstall Mod: {0}".FormatWith(report));

            if (report)
                return;

            try {
                await _updateManager.Value.HandleUninstall().ConfigureAwait(false);
            } catch (BusyStateHandler.BusyException) {
                _dialogManager.BusyDialog();
            }
        }

        [SmartAssembly.Attributes.ReportUsage]
        public async Task UninstallModSet(Collection collection) {
            if (collection == null)
                return;

            LibraryVM.ActiveItem = collection;

            var report =
                !(await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(string.Format("You are about to uninstall {0}\n{1}\nare you sure?",
                        collection.Name, String.Join(", ", collection.Items.Select(x => x.Name).ToArray())),
                        "Are you sure you want to uninstall the ModSet and all its modfolders?",
                        SixMessageBoxButton.YesNo) {
                            IgnoreContent = false,
                            GreenContent = "delete",
                            RedContent = "keep"
                        })).IsYes();

            UsageCounter.ReportUsage("Dialog - Uninstall ModSet: {0}".FormatWith(report));

            if (report)
                return;

            await TryUninstallModSet(collection).ConfigureAwait(false);
        }

        async Task TryUninstallModSet(Collection collection) {
            try {
                await _updateManager.Value.HandleUninstall().ConfigureAwait(false);
            } catch (BusyStateHandler.BusyException) {
                _dialogManager.BusyDialog();
            } finally {
                var custom = collection as CustomCollection;
                if (custom != null)
                    _contentManager.Value.RemoveCollection(custom);
            }
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ViewSupportAction(Collection x) {
            Tools.Generic.TryOpenUrl(x.SupportUrl);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ViewHomepageAction(IContent x) {
            Tools.Generic.TryOpenUrl(x.HomepageUrl);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void ViewChangelogAction(IContent x) {
            Tools.Generic.TryOpenUrl(x.GetChangelogUrl());
        }

        [SmartAssembly.Attributes.ReportUsage]
        [DoNotObfuscate]
        public void OpenNote(IHaveNotes x) {
            ShowNotes(x);
        }

        public Task<IAbsoluteFilePath> CreateIcon(Collection collection) {
            return _contentManager.Value.CreateIcon(collection);
        }

        public void SwitchGame(Game game) {
            ReplaceLibraryIfRequired(game);
        }

        #region IHandle events

        public void Handle(ActiveGameChangedForReal message) {
            if (message.Game.SupportsMods())
                LibraryVM.Setup();
        }

        public void Handle(GameContentInitialSynced message) {
            if (LibraryVM != null)
                LibraryVM.Setup();
        }

        public void Handle(SubGamesChanged message) {
            var libvm = LibraryVM;
            if (libvm != null && libvm.Game == message.Game)
                libvm.Reset();
        }

        #endregion
    }
}