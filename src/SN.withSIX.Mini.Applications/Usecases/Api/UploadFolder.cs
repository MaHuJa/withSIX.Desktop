using System;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class UploadFolder : IAsyncVoidCommand
    {
        public UploadFolder(string folder, Guid gameId, Guid contentId) {
            Folder = folder;
            GameId = gameId;
            ContentId = contentId;
        }

        public string Folder { get; }
        public Guid GameId { get; }
        public Guid ContentId { get; }
    }

    public class UploadFolderHandler : DbCommandBase, IAsyncVoidCommandHandler<UploadFolder>
    {
        readonly IFolderHandler _folderHandler;

        public UploadFolderHandler(IDbContextLocator dbContextLocator, IFolderHandler folderHandler)
            : base(dbContextLocator) {
            _folderHandler = folderHandler;
        }

        public async Task<UnitType> HandleAsync(UploadFolder request) {
            if (request.Folder.ToAbsoluteDirectoryPath() != _folderHandler.Folder)
                throw new ValidationException("This folder was not the one that was prepared!");
            // TODO:
            // - Get authorization key from account for this modfolder
            // - Upload the content
            // - Once completed, signal the API for processing.
            throw new NotImplementedException("TODO");
            return UnitType.Default;
        }
    }
}