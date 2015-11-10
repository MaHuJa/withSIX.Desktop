// <copyright company="SIX Networks GmbH" file="CreateProfileCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.UseCases.Profiles
{
    public class CreateProfileCommand : IAsyncRequest<Guid>
    {
        public CreateProfileCommand(string name, string color, Guid guid) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(color));
            Contract.Requires<ArgumentNullException>(guid != Guid.Empty);

            Name = name;
            Color = color;
            Guid = guid;
        }

        public string Name { get; }
        public string Color { get; }
        public Guid Guid { get; }
    }

    [StayPublic]
    public class CreateProfileCommandHandler : IAsyncRequestHandler<CreateProfileCommand, Guid>
    {
        readonly UserSettings _settings;
        readonly IUserSettingsStorage _storage;

        public CreateProfileCommandHandler(UserSettings settings, IUserSettingsStorage storage) {
            Contract.Requires<ArgumentNullException>(settings != null);
            _settings = settings;
            _storage = storage;
        }

        public async Task<Guid> HandleAsync(CreateProfileCommand request) {
            var newProfile = _settings.GameOptions.GameSettingsController.CreateProfile(request.Name, request.Color,
                request.Guid);

            await _storage.SaveNow().ConfigureAwait(false);

            return newProfile.Id;
        }
    }
}