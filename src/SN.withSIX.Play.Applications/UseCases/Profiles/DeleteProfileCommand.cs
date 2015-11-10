// <copyright company="SIX Networks GmbH" file="DeleteProfileCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.UseCases.Profiles
{
    public class DeleteProfileCommand : IRequest<UnitType>
    {
        public DeleteProfileCommand(Guid guid) {
            Contract.Requires<ArgumentNullException>(guid != Guid.Empty);

            Guid = guid;
        }

        public Guid Guid { get; }
    }

    [StayPublic]
    public class DeleteProfileCommandHandler : IRequestHandler<DeleteProfileCommand, UnitType>
    {
        readonly UserSettings _settings;

        public DeleteProfileCommandHandler(UserSettings settings) {
            _settings = settings;
        }

        public UnitType Handle(DeleteProfileCommand request) {
            _settings.GameOptions.GameSettingsController.DeleteProfile(request.Guid);
            return default(UnitType);
        }
    }
}