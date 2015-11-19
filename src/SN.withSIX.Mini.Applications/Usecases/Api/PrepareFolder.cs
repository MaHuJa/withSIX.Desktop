using System;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class PrepareFolder : IAsyncCommand<string> {}

    public class PrepareFolderHandler : DbCommandBase, IAsyncRequestHandler<PrepareFolder, string>
    {
        readonly IFolderHandler _folderHandler;
        readonly IDialogManager _dialogManager;

        public PrepareFolderHandler(IDbContextLocator dbContextLocator, IFolderHandler folderHandler, IDialogManager dialogManager) : base(dbContextLocator) {
            _folderHandler = folderHandler;
            _dialogManager = dialogManager;
        }

        public async Task<string> HandleAsync(PrepareFolder request) {
            // open dialog, ask for folder
            var folder = await _dialogManager.BrowseForFolderAsync("Select folder to upload to the withSIX network").ConfigureAwait(false);
            if (folder == null)
                throw new OperationCanceledException("The user cancelled the operation");
            // TODO: Restructure suggestions etc?
            _folderHandler.Folder = folder.ToAbsoluteDirectoryPath();
            return folder;
        }
    }
}