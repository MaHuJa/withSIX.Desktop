// <copyright company="SIX Networks GmbH" file="ViewModelFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel.Composition;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.ViewModels.Games.Popups;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public interface IViewModelFactory
    {
        ExportLifetimeContext<GameViewModel> CreateGame(Game game);
        ExportLifetimeContext<AboutViewModel> CreateAbout();
        ExportLifetimeContext<ApplicationLicensesViewModel> CreateApplicationLicenses();
        ExportLifetimeContext<AddRepositoryViewModel> CreateAddRepository();
        ExportLifetimeContext<SoftwareUpdateOverlayViewModelBase> CreateSoftwareUpdate();
        ExportLifetimeContext<ModLibraryViewModel> CreateModLibraryViewModel(Game game);

        ExportLifetimeContext<ServerLibraryViewModel> CreateServerLibraryViewModel(Game game,
            ServersViewModel serversViewModel);

        ExportLifetimeContext<MissionLibraryViewModel> CreateMissionLibraryViewModel(Game game);
        ExportLifetimeContext<AppOverlayViewModel> CreateApps();
    }

    public class ViewModelFactory : IApplicationService, IViewModelFactory
    {
        readonly ExportFactory<AboutViewModel> _aboutFactory;
        readonly IContentManager _contentList;
        readonly Lazy<ContentViewModel> _cvm;
        readonly IDialogManager _dm;
        readonly ExportFactory<ApplicationLicensesViewModel> _licenseFactory;
        readonly Lazy<LaunchManager> _lm;
        readonly Lazy<MissionsViewModel> _missions;
        readonly ExportFactory<MissionLibraryViewModel> _mlv2Factory;
        readonly ExportFactory<ModLibraryViewModel> _mlvFactory;
        readonly Lazy<ModsViewModel> _mods;
        readonly ExportFactory<AddRepositoryViewModel> _repositoryFactory;
        readonly Lazy<ServersViewModel> _servers;
        readonly UserSettings _settings;
        readonly ExportFactory<SoftwareUpdateOverlayViewModel> _suFactory;
        readonly Lazy<SoftwareUpdateSquirrelOverlayViewModel> _suFactory2;

        public ViewModelFactory(ExportFactory<AboutViewModel> aboutFactory,
            ExportFactory<ApplicationLicensesViewModel> licenseFactory,
            ExportFactory<AddRepositoryViewModel> repositoryFactory,
            ExportFactory<SoftwareUpdateOverlayViewModel> suFactory,
            Lazy<SoftwareUpdateSquirrelOverlayViewModel> suFactory2,
            ExportFactory<ModLibraryViewModel> mlvFactory, ExportFactory<MissionLibraryViewModel> mlv2Factory,
            UserSettings settings, IContentManager contentList, IDialogManager dm,
            Lazy<LaunchManager> lm,
            Lazy<ServersViewModel> servers,
            Lazy<ModsViewModel> mods,
            Lazy<ContentViewModel> cvm,
            Lazy<MissionsViewModel> missions) {
            _aboutFactory = aboutFactory;
            _licenseFactory = licenseFactory;
            _repositoryFactory = repositoryFactory;
            _suFactory = suFactory;
            _suFactory2 = suFactory2;
            _mlvFactory = mlvFactory;
            _mlv2Factory = mlv2Factory;
            _contentList = contentList;
            _settings = settings;
            _dm = dm;
            _lm = lm;
            _servers = servers;
            _mods = mods;
            _cvm = cvm;
            _missions = missions;
        }

        public ExportLifetimeContext<GameViewModel> CreateGame(Game game) {
            return
                new ExportLifetimeContext<GameViewModel>(
                    new GameViewModel(game, _servers.Value, _mods.Value, _missions.Value, _cvm),
                    TaskExt.NullAction);
        }

        public ExportLifetimeContext<AboutViewModel> CreateAbout() {
            return _aboutFactory.CreateExport();
        }

        public ExportLifetimeContext<ApplicationLicensesViewModel> CreateApplicationLicenses() {
            return _licenseFactory.CreateExport();
        }

        public ExportLifetimeContext<AddRepositoryViewModel> CreateAddRepository() {
            return _repositoryFactory.CreateExport();
        }

        public ExportLifetimeContext<SoftwareUpdateOverlayViewModelBase> CreateSoftwareUpdate() {
            return CreateSoftwareUpdateSquirrel();
        }

        public ExportLifetimeContext<ModLibraryViewModel> CreateModLibraryViewModel(Game game) {
            var scope = _mlvFactory.CreateExport();
            scope.Value.Game = game;
            return scope;
        }

        public ExportLifetimeContext<ServerLibraryViewModel> CreateServerLibraryViewModel(Game game,
            ServersViewModel serversViewModel) {
            return
                new ExportLifetimeContext<ServerLibraryViewModel>(
                    new ServerLibraryViewModel(game, new Lazy<ServersViewModel>(() => serversViewModel),
                        _contentList,
                        serversViewModel.ServerList, _lm.Value, _dm),
                    TaskExt.NullAction);
        }

        public ExportLifetimeContext<MissionLibraryViewModel> CreateMissionLibraryViewModel(Game game) {
            var mlvm = _mlv2Factory.CreateExport();
            mlvm.Value.Game = game;
            return mlvm;
        }

        public ExportLifetimeContext<AppOverlayViewModel> CreateApps() {
            return
                new ExportLifetimeContext<AppOverlayViewModel>(new AppOverlayViewModel(_settings, _dm),
                    TaskExt.NullAction);
        }

        ExportLifetimeContext<SoftwareUpdateOverlayViewModelBase> CreateSoftwareUpdateLegacy() {
            var exportLifetimeContext = _suFactory.CreateExport();
            return new ExportLifetimeContext<SoftwareUpdateOverlayViewModelBase>(exportLifetimeContext.Value,
                exportLifetimeContext.Dispose);
        }

        // Lame
        ExportLifetimeContext<SoftwareUpdateOverlayViewModelBase> CreateSoftwareUpdateSquirrel() {
            var exportLifetimeContext = _suFactory2;
            return new ExportLifetimeContext<SoftwareUpdateOverlayViewModelBase>(exportLifetimeContext.Value,
                () => { });
        }
    }
}