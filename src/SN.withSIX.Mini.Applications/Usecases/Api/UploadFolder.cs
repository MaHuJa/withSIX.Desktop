using System;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    /*
var client = w6Cheat.container.get(w6Cheat.containerObjects.client);
client.prepareFolder()
  .then(folder => client.uploadFolder({folder: folder, gameId: "9DE199E3-7342-4495-AD18-195CF264BA5B", contentId: "64de04c4-6751-4a3a-b915-7f3b1467f93f"}))
  .then(x => console.log("Complete!"))
  .catch(x => console.error("ERROR: ", x));
*/

    // TODO:
    // - Do not allow uploading while repository is in processing state
    // - Option to sign and process userconfigs on client??
    // - Verify structure
    // - Get authorization key and userId from account for this modfolder
    // - Once completed, signal the API for processing, the website can signal this??
    //   - Create synq package, update api etc.
    [ApiUserAction]
    public class UploadFolder : IAsyncVoidCommand
    {
        public UploadFolder(string folder, Guid userId, Guid gameId, Guid contentId) {
            Folder = folder;
            UserId = userId;
            GameId = gameId;
            ContentId = contentId;
        }

        public string Folder { get; }
        public Guid UserId { get; }
        public Guid GameId { get; }
        public Guid ContentId { get; }

        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class UploadFolderHandler : DbCommandBase, IAsyncVoidCommandHandler<UploadFolder>
    {
        readonly IFolderHandler _folderHandler;
        private readonly IQueueManager _queueManager;
        private readonly IRsyncLauncher _rsyncLauncher;

        public UploadFolderHandler(IDbContextLocator dbContextLocator, IFolderHandler folderHandler,
            IQueueManager queueManager, IRsyncLauncher rsyncLauncher)
            : base(dbContextLocator) {
            _folderHandler = folderHandler;
            _queueManager = queueManager;
            _rsyncLauncher = rsyncLauncher;
        }

        public async Task<UnitType> HandleAsync(UploadFolder request) {
            if (!request.Folder.ToAbsoluteDirectoryPath().Equals(_folderHandler.Folder))
                throw new ValidationException("This folder was not the one that was prepared!");

            await
                _queueManager.AddToQueue("Upload " + request.Folder.ToAbsoluteDirectoryPath().DirectoryName,
                    (progress, ct) => UploadFolder(request, ct, progress)).ConfigureAwait(false);

            return UnitType.Default;
        }

        // TODO: Split to service
        async Task UploadFolder(UploadFolder request, CancellationToken token, Action<ProgressState> progress) {
            var auth = new AuthInfo(request.UserName, request.Password);

            const string host = "staging.sixmirror.com";
            var folderPath =
                $"{request.UserId.ToString().ToLower()}/{request.GameId.ToString().ToLower()}/{request.ContentId.ToString().ToLower()}";
            Environment.SetEnvironmentVariable("RSYNC_PASSWORD", auth.Password);

            var tp = new TransferProgress();
            using (new Monitor(tp, progress)) {
                var result = await
                    _rsyncLauncher.RunAndProcessAsync(tp,
                        request.Folder, // "."
                        $"rsync://{auth.UserName}@{host}/{folderPath}", token,
                        new RsyncOptions {AdditionalArguments = {"-avz"}}) // , WorkingDirectory = request.Folder
                        .ConfigureAwait(false);
                MainLog.Logger.Debug("Output" + result.StandardOutput + "\nError " + result.StandardError);
                if (result.ExitCode > 0)
                    throw new Exception("error while executing " + result.ExitCode);
            }
        }

        class Monitor : IDisposable
        {
            private readonly Action<ProgressState> _action;
            private readonly ITransferProgress _progress;
            private TimerWithoutOverlap _timer;

            public Monitor(ITransferProgress progress, Action<ProgressState> action) {
                _progress = progress;
                _action = action;
                _timer = new TimerWithoutOverlap(TimeSpan.FromMilliseconds(500), OnElapsed);
            }

            public void Dispose() {
                _timer.Dispose();
                _timer = null;
            }

            private void OnElapsed() {
                _action(new ProgressState(_progress.Progress, _progress.Speed, "Uploading"));
            }
        }

        class AuthInfo
        {
            public AuthInfo(string authInfo) {
                var authSplit = authInfo.Split(':');
                UserName = authSplit[0];
                Password = authSplit[1];
            }

            public AuthInfo(string userName, string password) {
                UserName = userName;
                Password = password;
            }

            public string UserName { get; }
            public string Password { get; }
        }
    }
}