// <copyright company="SIX Networks GmbH" file="UploadNewCollectionImageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class UploadNewCollectionImageCommand : IAsyncRequest<UnitType>
    {
        public UploadNewCollectionImageCommand(Guid id, IAbsoluteFilePath filePath) {
            Id = id;
            FilePath = filePath;
        }

        public Guid Id { get; }
        public IAbsoluteFilePath FilePath { get; }
    }

    public class UploadNewCollectionImageCommandHandler :
        IAsyncRequestHandler<UploadNewCollectionImageCommand, UnitType>
    {
        readonly IConnectApiHandler _api;
        readonly IContentManager _contentList;

        public UploadNewCollectionImageCommandHandler(IConnectApiHandler api, IContentManager contentList) {
            _api = api;
            _contentList = contentList;
        }

        public Task<UnitType> HandleAsync(UploadNewCollectionImageCommand request) {
            var size = request.FilePath.FileInfo.Length;
            SizeExtensions.DefaultContentLengthRange.VerifySize(size);
            var collection = _contentList.CustomCollections
                .First(x => x.Id == request.Id);
            return collection.UploadAvatar(request.FilePath, _api).Void();
        }
    }
}