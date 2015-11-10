// <copyright company="SIX Networks GmbH" file="MissionsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Events;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    [DoNotObfuscate]
    public class MissionsViewModel : LibraryModuleViewModel,
        IHandle<GameContentInitialSynced>, IHandle<RequestPublishMission>,
        IHandle<ActiveGameChangedForReal>
    {
        readonly IViewModelFactory _factory;
        readonly Func<MissionUploadViewModel> _uploadViewModelFactory;
        MissionLibraryViewModel _libraryVm;
        protected MissionsViewModel() {}

        public MissionsViewModel(Lazy<ModsViewModel> modsViewModel,
            MissionInfoOverlayViewModel miovm, IViewModelFactory factory, IContentManager missionList,
            UserSettings settings,
            Func<MissionUploadViewModel> uploadViewModelFactory) {
            _factory = factory;
            UserSettings = settings;
            _uploadViewModelFactory = uploadViewModelFactory;
            ModuleName = ControllerModules.MissionBrowser;

            MissionInfoOverlayViewModel = miovm;
            MissionList = missionList;
            DisplayName = "Missions";
            Mods = modsViewModel;
        }

        public Lazy<ModsViewModel> Mods { get; private set; }
        public MissionInfoOverlayViewModel MissionInfoOverlayViewModel { get; }
        public MissionUploadViewModel MissionUploadViewModel { get; protected set; }
        public IContentManager MissionList { get; set; }
        public UserSettings UserSettings { get; }
        public MissionLibraryViewModel LibraryVM
        {
            get { return _libraryVm; }
            set { SetProperty(ref _libraryVm, value); }
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            this.WhenAnyValue(x => x.LibraryVM.IsLoading).Subscribe(x => ProgressState.Active = x);
            CreateLibrary(DomainEvilGlobal.SelectedGame.ActiveGame);
        }

        [ReportUsage]
        public async Task ShowUploadOverlay(MissionBase content) {
            var missions =
                await MissionList.GetMyMissions(DomainEvilGlobal.SelectedGame.ActiveGame);
            var vm = _uploadViewModelFactory();
            vm.ExistingMissions = missions.Select(x => x.Name).ToList();
            vm.UploadNewMission = !missions.Any();
            vm.Content = content;

            MissionUploadViewModel = vm;
            ShowOverlay(MissionUploadViewModel);
        }

        [DoNotObfuscate, ReportUsage]
        public void SelectNoMission() {
            LibraryVM.ActiveItem = null;
        }

        [ReportUsage]
        public void SwitchMissionListAct(object x) {
            if (!IsActive)
                Open();
        }

        void CreateLibrary(Game game) {
            if (game.SupportsMissions()) {
                UiHelper.TryOnUiThread(
                    () => {
                        var missionLibraryViewModel = _factory.CreateMissionLibraryViewModel(game).Value;
                        ((IActivate) missionLibraryViewModel).Activate();
                        LibraryVM = missionLibraryViewModel;
                    });
            } else
                LibraryVM = null;
        }

        [ReportUsage]
        public void MissionVersion() {
            ShowOverlay(MissionInfoOverlayViewModel);
        }

        public Mission GetSelectedMission() {
            var lib = LibraryVM;
            if (lib == null)
                return null;
            var selected = lib.SelectedItem;
            if (selected == null)
                return null;
            var content = selected.SelectedItem as Mission;
            return content;
        }

        public void SwitchGame(Game game) {
            CreateLibrary(game);
        }

        #region IHandle events

        public void Handle(ActiveGameChangedForReal message) {
            if (message.Game.SupportsMissions())
                LibraryVM.Setup();
        }

        public void Handle(GameContentInitialSynced message) {
            if (LibraryVM != null)
                LibraryVM.Setup();
        }

        public async void Handle(RequestPublishMission message) {
            Open();
            await ShowUploadOverlay(message.Mission).ConfigureAwait(false);
        }

        #endregion
    }
}