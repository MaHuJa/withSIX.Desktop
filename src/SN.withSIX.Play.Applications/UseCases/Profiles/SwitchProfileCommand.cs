// <copyright company="SIX Networks GmbH" file="SwitchProfileCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.UseCases.Profiles
{
    public class SwitchProfileCommand : IRequest<UnitType>
    {
        public SwitchProfileCommand(Guid guid) {
            Contract.Requires<ArgumentNullException>(guid != Guid.Empty);

            Guid = guid;
        }

        public Guid Guid { get; }
    }

    public class SwitchProfileByNameCommand : IRequest<UnitType>
    {
        public SwitchProfileByNameCommand(string name) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Name = name;
        }

        public string Name { get; }
    }


    [StayPublic]
    public class SwitchProfileCommandHandler : IRequestHandler<SwitchProfileCommand, UnitType>,
        IRequestHandler<SwitchProfileByNameCommand, UnitType>
    {
        readonly UserSettings _settings;

        public SwitchProfileCommandHandler(UserSettings settings) {
            _settings = settings;
        }

        public UnitType Handle(SwitchProfileByNameCommand request) {
            _settings.GameOptions.GameSettingsController.ActivateProfile(request.Name);
            return default(UnitType);
        }

        public UnitType Handle(SwitchProfileCommand request) {
            _settings.GameOptions.GameSettingsController.ActivateProfile(request.Guid);
            return default(UnitType);
        }
    }
}