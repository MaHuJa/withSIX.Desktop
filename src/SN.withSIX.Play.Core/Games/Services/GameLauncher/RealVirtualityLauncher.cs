// <copyright company="SIX Networks GmbH" file="RealVirtualityLauncher.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Api.Models;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;

namespace SN.withSIX.Play.Core.Games.Services.GameLauncher
{
    class RealVirtualityLauncher : GameLauncher, IRealVirtualityLauncher
    {
        readonly IAbsoluteDirectoryPath _parPath;
        readonly IFileWriter _writer;

        public RealVirtualityLauncher(IMediator mediator, IGameLauncherProcess processManager,
            IPathConfiguration pathConfiguration, IFileWriter writer)
            : base(mediator, processManager) {
            Contract.Requires<ArgumentNullException>(writer != null);
            _writer = writer;
            _parPath = pathConfiguration.LocalDataPath.GetChildDirectoryWithName("games");
        }

        public Task<Process> Launch(LaunchGameWithSteamInfo spec) {
            return LaunchInternal(spec);
        }

        public Task<Process> Launch(LaunchGameInfo spec) {
            return LaunchInternal(spec);
        }

        public Task<Process> Launch(LaunchGameWithSteamLegacyInfo spec) {
            return LaunchInternal(spec);
        }

        public async Task<IAbsoluteFilePath> WriteParFile(WriteParFileInfo info) {
            var filePath = GetFilePath(info);
            this.Logger().Info("Writing par file at: {0}, with:\n{1}", filePath, info.Content);
            await _writer.WriteFileAsync(filePath.ToString(), info.Content, Encoding.Default).ConfigureAwait(false);
            return filePath;
        }

        IAbsoluteFilePath GetFilePath(WriteParFileInfo info) {
            return
                _parPath.GetChildDirectoryWithName(new ShortGuid(info.GameId).ToString())
                    .GetChildFileWithName(GetFileName(info));
        }

        static string GetFileName(WriteParFileInfo info) {
            var additionalIdentifier = info.AdditionalIdentifier == null ? null : "_" + info.AdditionalIdentifier;
            return "par" + additionalIdentifier + ".txt";
        }
    }
}