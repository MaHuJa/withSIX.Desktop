using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Mini.Applications.Services;
using SN.withSIX.Mini.Applications.Services.Infra;

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
        readonly IProcessManager _processManager;

        public UploadFolderHandler(IDbContextLocator dbContextLocator, IFolderHandler folderHandler,
            IProcessManager processManager)
            : base(dbContextLocator) {
            _folderHandler = folderHandler;
            _processManager = processManager;
        }

        public async Task<UnitType> HandleAsync(UploadFolder request) {
            if (!request.Folder.ToAbsoluteDirectoryPath().Equals(_folderHandler.Folder))
                throw new ValidationException("This folder was not the one that was prepared!");

            // TODO
            var auth =
                new AuthInfo(
                    "YpQgYsURsZB99QzjbRsqJpZR9hVtzuhQwXk9Lv3QFVSDxyXfCvwtphKKLCqTBrWu:v4xzBTEbSG9Y27DhcfGXNTvym9GRsKGuEELNvD9zzAcXPAhVeF7nJtH3VmgngzP7");
            var userId = new Guid("ac03f83b-858c-48c0-a51d-232298a2d265");

            await UploadFolder(auth, request.Folder, userId, request.GameId, request.ContentId).ConfigureAwait(false);

            return UnitType.Default;
        }

        // TODO: Split to service
        async Task UploadFolder(AuthInfo auth, string folder, Guid userId, Guid gameId, Guid contentId) {
            const string host = "staging.sixmirror.com";
            var rsyncTool = Common.Paths.ToolCygwinBinPath.GetChildFileWithName("rsync.exe");
            var arguments =
                $"--delete -avz . rsync://{auth.UserName}@{host}/{userId.ToString().ToLower()}/{gameId.ToString().ToLower()}/{contentId.ToString().ToLower()}";
            Environment.SetEnvironmentVariable("RSYNC_PASSWORD", auth.Password);
            var result = await
                _processManager.LaunchAndGrabAsync(
                    new BasicLaunchInfo(new ProcessStartInfo(rsyncTool.ToString(), arguments) {
                        WorkingDirectory = folder
                    }))
                    .ConfigureAwait(false);
            MainLog.Logger.Debug("Output" + result.StandardOutput + "\nError " + result.StandardError);
            if (result.ExitCode > 0)
                throw new Exception("error while executing " + result.ExitCode);
        }

        class AuthInfo
        {
            public AuthInfo(string authInfo) {
                var authSplit = authInfo.Split(':');
                UserName = authSplit[0];
                Password = authSplit[1];
            }

            public string UserName { get; }
            public string Password { get; }
        }
    }
}