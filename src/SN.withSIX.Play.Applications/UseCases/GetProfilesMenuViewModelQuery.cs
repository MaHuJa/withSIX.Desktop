// <copyright company="SIX Networks GmbH" file="GetProfilesMenuViewModelQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Profiles;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Popups;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class GetProfilesMenuViewModelQuery : IRequest<ProfilesMenuViewModel> {}

    [StayPublic]
    public class GetProfilesMenuViewModelQueryHandler :
        IRequestHandler<GetProfilesMenuViewModelQuery, ProfilesMenuViewModel>
    {
        readonly Func<ProfilesMenuViewModel> _createVm;
        readonly IGameMapperConfig _mapper;
        readonly UserSettings _settings;

        public GetProfilesMenuViewModelQueryHandler(Func<ProfilesMenuViewModel> createVm, UserSettings settings,
            IGameMapperConfig mapper) {
            _createVm = createVm;
            _settings = settings;
            _mapper = mapper;
        }

        public ProfilesMenuViewModel Handle(GetProfilesMenuViewModelQuery request) {
            // TODO: Does a query make sense if all it really is is a factory?
            // Either the Container could be configured to do this, or e.g the Shared ViewModelFactory, although that would blow it up even further
            // At the same time, ViewModels generally should be re-usable, so having it's own use case handler isn't all that bad on second sight :)
            var vm = _createVm();
            var profiles =
                _settings.GameOptions.GameSettingsController.Profiles.CreateDerivedCollection(
                    x => _mapper.Map<ProfileDataModel>(x));
            profiles.EnableCollectionSynchronization(new object());
            vm.Profiles = profiles;

            // TODO: How would this one get disposed? The Profiles collection can be taken care of in a Dispose method on the VM, but this one would need to be separately registered?
            // One way could be to return an ExportLifetimeContext instead, which would clean this up on Dispose; would that be the right tool for the job though?
            DomainEvilGlobal.Settings.WhenAnyValue(x => x.GameOptions.GameSettingsController.ActiveProfile)
                .Subscribe(x => vm.ActiveProfile = vm.Profiles.First(y => y.Id == x.Id));

            return vm;
        }
    }
}