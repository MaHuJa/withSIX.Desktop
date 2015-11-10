// <copyright company="SIX Networks GmbH" file="GenerateNewCollectionImageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class GenerateNewCollectionImageCommand : IAsyncRequest<UnitType>
    {
        public GenerateNewCollectionImageCommand(Guid id) {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GenerateNewCollectionImageCommandHandler :
        IAsyncRequestHandler<GenerateNewCollectionImageCommand, UnitType>
    {
        readonly IConnectApiHandler _api;
        readonly IContentManager _contentList;

        public GenerateNewCollectionImageCommandHandler(IConnectApiHandler api, IContentManager contentList) {
            _api = api;
            _contentList = contentList;
        }

        public Task<UnitType> HandleAsync(GenerateNewCollectionImageCommand request) {
            var collection = _contentList.CustomCollections.First(x => x.Id == request.Id);
            return collection.GenerateNewAvatar(_api).Void();
        }
    }
}