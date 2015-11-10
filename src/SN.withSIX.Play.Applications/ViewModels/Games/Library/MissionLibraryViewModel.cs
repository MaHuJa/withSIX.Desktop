// <copyright company="SIX Networks GmbH" file="MissionLibraryViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using ReactiveUI;
using SmartAssembly.Attributes;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Library
{
    public static class BusyDialogExtensions
    {
        public static void BusyDialog(this IDialogManager dialogManager) {
            UsageCounter.ReportUsage("Dialog - Cannot perform action; already busy");
            dialogManager.MessageBoxSync(new MessageBoxDialogParams(
                "Cannot perform action; already busy.\nPlease wait until current action has completed",
                "Warning, cannot perform action while already busy"));
        }
    }

    [DoNotObfuscate]
    public class MissionLibraryViewModel : ContentLibraryRootViewModel, IDropTarget, ITransient
    {
        static readonly string[] missionSubFolders = {MissionFolders.SpMissions, MissionFolders.MpMissions};
        static readonly Dictionary<Guid, string> sixPlayDic = new Dictionary<Guid, string> {
            {GameUuids.Arma3, "@six_play_a3"},
            {GameUuids.Arma2, "@six_play_a2"},
            {GameUuids.Arma2Oa, "@six_play_a2"},
            {GameUuids.Arma2Co, "@six_play_a2"}
        };
        readonly IContentManager _contentList;
        readonly IDialogManager _dialogManager;
        readonly IEventAggregator _eventBus;
        readonly Lazy<LaunchManager> _launchManager;
        readonly IContentManager _missionList;
        readonly MissionsViewModel _missionsViewModel;
        readonly UserSettings _settings;
        readonly object _setupLock = new Object();
        readonly IUpdateManager _updateManager;
        MissionLibrarySetup _librarySetup;

        public MissionLibraryViewModel(MissionsViewModel missionsViewModel, IEventAggregator eventBus,
            UserSettings settings, IDialogManager dialogManager,
            Lazy<LaunchManager> launchManager,
            IUpdateManager updateManager, IContentManager contentList,
            ExportFactory<PickContactViewModel> pickContactFactory)
            : base(pickContactFactory, missionsViewModel) {
            SearchItem = new MissionSearchContentLibraryItemViewModel(this);
            _missionsViewModel = missionsViewModel;
            _missionList = contentList;
            _eventBus = eventBus;
            _settings = settings;
            _dialogManager = dialogManager;
            _launchManager = launchManager;
            _updateManager = updateManager;
            _contentList = contentList;

            Comparer = new ModSearchItemComparer();

            ViewType = settings.MissionOptions.ViewType;
            this.ObservableForProperty(x => x.ViewType)
                .Select(x => x.Value)
                .BindTo(settings, s => s.MissionOptions.ViewType);

            LocalMissionContextMenu = new LocalMissionFolderContextMenu(this);
        }

        protected MissionLibraryViewModel() : base(null, null) {}
        MissionLibrarySetup LibrarySetup
        {
            get { return _librarySetup; }
            set
            {
                _librarySetup = value;
                SetUp = value;
            }
        }
        public LocalMissionFolderContextMenu LocalMissionContextMenu { get; }
        protected internal Game Game { get; set; }
        public void DragOver(IDropInfo dropInfo) {}
        public void Drop(IDropInfo dropInfo) {}

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WhenAnyValue(x => x.Game.CalculatedSettings.Mission)
                .Subscribe(x => ActiveItem = x);

            this.WhenAnyValue(x => x.SelectedItem)
                .Subscribe(OnSelectedItemChanged);

            this.WhenAnyValue(x => x.SelectedItem.SelectedItem)
                .Where(x => x != null)
                .Subscribe(OnSelectedItem2Changed);

            this.WhenAnyValue(x => x.ActiveItem)
                .Skip(1)
                .Subscribe(OnActiveItemChanged);
        }

        public async Task DownloadMission(Mission mission) {
            try {
                await _updateManager.DownloadMission(mission).ConfigureAwait(false);
            } catch (BusyStateHandler.BusyException) {
                _dialogManager.BusyDialog();
            }
        }

        void OnActiveItemChanged(IContent content) {
            Game.CalculatedSettings.Mission = (MissionBase) content;
        }

        protected override bool ApplySearchFilter(object obj) {
            var mission = obj as Mission;
            var searchText = SearchText;
            if (mission != null) {
                return (mission.Name.NullSafeContainsIgnoreCase(searchText) ||
                        mission.FileName.NullSafeContainsIgnoreCase(searchText) ||
                        mission.FullName.NullSafeContainsIgnoreCase(searchText) ||
                        mission.Author.NullSafeContainsIgnoreCase(searchText)
                        || mission.Tags.Any(x => x.NullSafeContainsIgnoreCase(searchText)));
            }

            var mb = obj as MissionBase;
            return mb != null && (mb.Name.NullSafeContainsIgnoreCase(searchText) ||
                                  mb.Author.NullSafeContainsIgnoreCase(searchText));
        }

        public override void Setup() {
            lock (_setupLock) {
                IsLoading = true;
                ItemsView = null;
                if (LibrarySetup != null)
                    LibrarySetup.Dispose();
                LibrarySetup = new MissionLibrarySetup(this, Game, _missionList, _settings, _eventBus);
                SetupGroups();
                InitialSelectedItem();
                IsLoading = false;
            }
        }

        protected override void InitialSelectedItem() {
            SelectedItem = LibrarySetup.Items.FirstOrDefault();
        }

        void OnSelectedItem2Changed(IHierarchicalLibraryItem x) {
            var missionBase = x as MissionBase;
            if (missionBase != null)
                HandleOverlay(missionBase);
        }

        void HandleOverlay(MissionBase mission) {
            if (_missionsViewModel.MissionInfoOverlayViewModel.IsActive &&
                !(AllowInfoOverlay(mission)))
                _missionsViewModel.MissionInfoOverlayViewModel.TryClose();

            if (_missionsViewModel.MissionInfoOverlayViewModel.IsActive &&
                !mission.Controller.HasMultipleVersions())
                _missionsViewModel.MissionInfoOverlayViewModel.TryClose();
        }

        static bool AllowInfoOverlay(MissionBase mission) {
            return mission is Mission && !mission.IsLocal;
        }

        void OnSelectedItemChanged(IHierarchicalLibraryItem x) {
            if (x == null) {
                ContextMenu = null;
                return;
            }

            // TODO: Just store the menus on the libraryitemviewmodels ??
            var lmission = x as ContentLibraryItemViewModel<LocalMissionsContainer>;
            if (lmission != null) {
                LocalMissionContextMenu.ShowForItem(lmission);
                ContextMenu = LocalMissionContextMenu;
                return;
            }

            ContextMenu = null;
        }

        async Task RemoveLocalMissions(LocalMissionsContainer localMissions) {
            _missionList.LocalMissionsContainers.Remove(localMissions);
            DomainEvilGlobal.Settings.RaiseChanged();
            //localMissions.Dispose();
        }

        [DoNotObfuscate]
        public void ActivateItem(IContent mission) {
            SelectedItem.SelectedItem = mission;
            ActiveItem = mission;
        }

        [DoNotObfuscate]
        public void RemoveLibraryItem() {
            var item = SelectedItem.FindItem<ContentLibraryItemViewModel>();
            if (item != null)
                RemoveLibraryItem(item);
        }

        [DoNotObfuscate]
        public override async Task RemoveLibraryItem(ContentLibraryItemViewModel content) {
            var localMissions = content as ContentLibraryItemViewModel<LocalMissionsContainer>;
            if (localMissions != null)
                await RemoveLocalMissions(localMissions.Model).ConfigureAwait(false);
        }

        [DoNotObfuscate]
        public void ShowInfo(IContent content) {
            SelectedItem.SelectedItem = content;
            BrowserHelper.TryOpenUrlIntegrated(content.ProfileUrl());
        }

        [DoNotObfuscate]
        public void ShowDependency(KeyValuePair<string, string> dependency) {
            BrowserHelper.TryOpenUrlIntegrated(Tools.Transfer.JoinUri(CommonUrls.PlayUrl, Game.MetaData.Slug,
                "mods",
                dependency.Key.Sluggify()));
        }

        void SetupGroups() {
            LibrarySetup.LocalGroup.AddCommand.RegisterAsyncTask(AddLocalFolder).Subscribe();
        }

        async Task AddLocalFolder() {
            var path = await _dialogManager.BrowseForFolderAsync();
            if (string.IsNullOrWhiteSpace(path))
                return;

            await Task.Run(async () => {
                if (
                    LibrarySetup.GetLocalMissions()
                        .Any(x => Tools.FileUtil.ComparePathsOsCaseSensitive(x.Model.Path, path))) {
                    await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("You've already added this folder"));
                    return;
                }

                if (!GetMissionSubfolders(path).Any(Directory.Exists)) {
                    await _dialogManager.MessageBoxAsync(
                        new MessageBoxDialogParams(
                            "Please select a folder that contains a Missions and/or MPMissions subfolder",
                            "Invalid folder, aborting"));
                    return;
                }

                var item = LibrarySetup.CreateLocalItem(Path.GetFileName(path), false, path);
                _missionList.LocalMissionsContainers.Add(item.Model);
                DomainEvilGlobal.Settings.RaiseChanged();
            });
        }

        static IEnumerable<string> GetMissionSubfolders(string path) {
            return missionSubFolders.Select(subDir => Path.Combine(path, subDir));
        }

        public void MoveLocalMissionDirectory(ContentLibraryItemViewModel<LocalMissionsContainer> getLibraryItem) {
            _eventBus.PublishOnUIThread(new RequestGameSettingsOverlay(Game.Id));
        }

        public Task PublishMission(MissionBase mission) {
            return _missionsViewModel.ShowUploadOverlay(mission);
        }

        public Task LaunchMission(Mission mission) {
            ActiveItem = mission;
            return _launchManager.Value.StartGame();
        }

        public async Task OpenMissionInGameEditor(MissionFolder mission) {
            await HandleSixPlay().ConfigureAwait(false);
            ActiveItem = mission;
            await _launchManager.Value.StartGame().ConfigureAwait(false);
        }

        [DoNotObfuscate]
        public void ShowVersion(IContent mod) {
            SelectedItem.SelectedItem = mod;
            _missionsViewModel.MissionVersion();
        }

        [DoNotObfuscate]
        public void AddOwnMission() {
            BrowserHelper.TryOpenUrlIntegrated("http://withsix.com/getting-started-publishing");
        }

        async Task HandleSixPlay() {
            var sixPlay = GetSixPlay(Game);
            if (sixPlay != null) {
                var mods = _missionsViewModel.Mods.Value;
                var mod = mods.ContentManager.FindMod(sixPlay);
                var currentModSet = Game.CalculatedSettings.Collection;
                if (currentModSet != null) {
                    if (!currentModSet.EnabledMods.Contains(mod)) {
                        var modSet = mods.ContentManager.CloneCollection(currentModSet);
                        modSet.AddModAndUpdateState(mod, _contentList);
                        mods.LibraryVM.ActiveItem = modSet;
                    }
                } else
                    mods.LibraryVM.ActiveItem = mod;
                try {
                    await
                        _updateManager.HandleConvertOrInstallOrUpdate().ConfigureAwait(false);
                } catch (BusyStateHandler.BusyException) {
                    _dialogManager.BusyDialog();
                }
            }
        }

        static string GetSixPlay(Game game) {
            return sixPlayDic.ContainsKey(game.Id) ? sixPlayDic[game.Id] : null;
        }
    }
}