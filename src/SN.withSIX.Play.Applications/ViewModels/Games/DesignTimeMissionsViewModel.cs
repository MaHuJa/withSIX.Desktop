// <copyright company="SIX Networks GmbH" file="DesignTimeMissionsViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Caliburn.Micro;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class DesignTimeMissionsViewModel : MissionsViewModel, IDesignTimeViewModel
    {
        public DesignTimeMissionsViewModel()
            : base(new Lazy<ModsViewModel>(),
                new MissionInfoOverlayViewModel(new Lazy<MissionsViewModel>()), IoC.Get<IViewModelFactory>(),
                IoC.Get<IContentManager>(), IoC.Get<UserSettings>(), null) {
            MissionList.Missions.Add(new Mission(Guid.Empty) {
                Name = "Test mission",
                GameId = new Guid("abc"),
                FileName = "test123"
            });
            LibraryVM = new DesignTimeMissionLibraryViewModel();
        }
    }

    public class DesignTimeMissionLibraryViewModel : MissionLibraryViewModel, IDesignTimeViewModel
    {
        public DesignTimeMissionLibraryViewModel() {
            var lib =
                new MissionLibraryItemViewModel<BuiltInContentContainer>(this, new BuiltInContentContainer("Browse"));
            var mission = new Mission(Guid.Empty) {Name = "Test mission", Author = "Test Author"};
            lib.Items.Add(mission);
            lib.SelectedItem = mission;

            // TODO
            //CreateItemsView(new ReactiveList<ContentLibraryItem>(new[] {lib}), new LibraryGroup[0]);
            SelectedItem = lib;
        }
    }
}