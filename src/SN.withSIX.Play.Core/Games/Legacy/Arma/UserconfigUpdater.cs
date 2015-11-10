// <copyright company="SIX Networks GmbH" file="UserconfigUpdater.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using System.IO;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma
{
    public class UserconfigUpdater : UserconfigProcessorBase
    {
        public void ProcessMissingFiles(string path, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath uconfig,
            IAbsoluteDirectoryPath uconfigPath) {
            if (uconfigPath.Exists)
                this.Logger().Info("Userconfig folder already exists, checking for new files only");
            TryUserconfigUpdate(path, gamePath, uconfig, uconfigPath);
        }

        void TryUserconfigUpdate(string path, IAbsoluteDirectoryPath gamePath, IAbsoluteDirectoryPath uconfig,
            IAbsoluteDirectoryPath uconfigPath) {
            if (!ConfirmUserconfigIsNotFile(uconfig.ToString()))
                return;

            if (Directory.Exists(path))
                TryUserconfigDirectory(path.ToAbsoluteDirectoryPath(), uconfigPath);
            else if (File.Exists(path))
                TryUserconfigUnpack(path.ToAbsoluteFilePath(), gamePath);
        }

        static void TryUserconfigUnpack(IAbsoluteFilePath path, IAbsoluteDirectoryPath gamePath) {
            try {
                Tools.Compression.UnpackRetryUpdater(path, gamePath);
            } catch (Win32Exception ex) {
                if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                    throw;
                throw ex.HandleUserCancelled();
            }
        }

        void TryUserconfigDirectory(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath uconfigPath) {
            try {
                Tools.FileUtil.Ops.CopyDirectoryWithUpdaterFallbackAndRetry(path, uconfigPath);
            } catch (Win32Exception ex) {
                if (ex.NativeErrorCode != Win32ErrorCodes.ERROR_CANCELLED_ELEVATION)
                    throw;
                throw ex.HandleUserCancelled();
            } catch (IOException e) {
                this.Logger().FormattedWarnException(e);
            }
        }
    }
}