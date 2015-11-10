// <copyright company="SIX Networks GmbH" file="ShowNewProfileDialogQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.DataModels.Profiles;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.ViewModels.Dialogs;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class ShowNewProfileDialogQuery : IRequest<NewProfileViewModel> {}

    [StayPublic]
    public class ShowNewProfileDialogCommandHandler : IRequestHandler<ShowNewProfileDialogQuery, NewProfileViewModel>
    {
        readonly Func<NewProfileViewModel> _createVm;
        readonly IGameMapperConfig _mapper;
        readonly UserSettings _settings;

        public ShowNewProfileDialogCommandHandler(Func<NewProfileViewModel> createVm, UserSettings settings,
            IGameMapperConfig mapper) {
            _createVm = createVm;
            _settings = settings;
            _mapper = mapper;
        }

        public static string[] Accents { get; set; }

        public NewProfileViewModel Handle(ShowNewProfileDialogQuery request) {
            var vm = _createVm();
            var profiles =
                _settings.GameOptions.GameSettingsController.Profiles.CreateDerivedCollection(
                    x => _mapper.Map<ProfileDataModel>(x));
            profiles.EnableCollectionSynchronization(new object());
            vm.Colors = new List<string>(new[] {
                SixColors.SixBlue
            }.Concat(Accents));
            vm.BaseProfiles = profiles;
            return vm;
        }
    }
}