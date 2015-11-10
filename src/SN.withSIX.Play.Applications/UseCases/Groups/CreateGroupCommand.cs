// <copyright company="SIX Networks GmbH" file="CreateGroupCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Applications.UseCases.Groups
{
    public class CreateGroupCommand : IAsyncRequest<Guid>, ICreateGroupInfo
    {
        public CreateGroupCommand(string name, string description, Uri url, IAbsoluteFilePath logoFilename,
            IAbsoluteFilePath backgroundFilename) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

            Name = name;
            Description = description;
            Homepage = url;
            LogoFilename = logoFilename;
            BackgroundFilename = backgroundFilename;
        }

        public IAbsoluteFilePath LogoFilename { get; }
        public IAbsoluteFilePath BackgroundFilename { get; }
        public string Name { get; }
        public string Description { get; }
        public Uri Homepage { get; }
    }

    [StayPublic]
    public class CreateGroupCommandHandler : IAsyncRequestHandler<CreateGroupCommand, Guid>
    {
        readonly IConnectApiHandler _apiHandler;

        public CreateGroupCommandHandler(IGameMapperConfig gameMapper, IConnectApiHandler apiHandler) {
            Contract.Requires<ArgumentNullException>(gameMapper != null);
            Contract.Requires<ArgumentNullException>(apiHandler != null);

            _apiHandler = apiHandler;
        }

        public Task<Guid> HandleAsync(CreateGroupCommand request) {
            return _apiHandler.CreateGroup(request, request.LogoFilename,
                request.BackgroundFilename);
        }
    }
}