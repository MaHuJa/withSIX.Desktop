// <copyright company="SIX Networks GmbH" file="IronFrontInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.ExternalTools;
using SN.withSIX.Sync.Core.Legacy.Status;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Mini.Plugin.Arma.Services
{
    public class IronFrontInstaller : IDomainService
    {
        const long RequiredFreeSpace = 7*FileSizeUnits.GB;
        const long RequiredTempFreeSpace = 2*FileSizeUnits.GB;
        const string CdnPath = "cdn/patches/ironfront";
        // TODO: use any of our mirrors like c1-de, c1-us, etc...
        static readonly Version requiredVersion = new Version(1, 65);
        static readonly VersionDescription game111Patch =
            new VersionDescription("1.11", "4d8c8dbfc307e0b66d23a6b33e3f7a6a13229db7");
        static readonly VersionDescription game112Patch =
            new VersionDescription("1.12", "ebf606a0b28a3854ff2d342333ad096854835b6d");
        static readonly VersionDescription dlc107Patch =
            new VersionDescription("1.07", "c83ac97671642189047962e6c8fa400e2574932e");
        static readonly IList<GamePatch> communityGamePatches = new List<GamePatch> {
            new GamePatch(
                new VersionDescription("1.05", "c6e0fbdd35f238d7ea36855c6ef00b70d5baa169"),
                game112Patch),
            // ARMA2CO only
            new GamePatch(
                new VersionDescription("1.08", "739bef1537183668ac92315267ec8764caa3b62d"),
                game111Patch),
            new GamePatch(
                game111Patch,
                game112Patch),
            new GamePatch(
                game112Patch,
                new VersionDescription("1.13", "3fc4609179427e98fa8d8dafbd3410eae3cd3d09"))
        };
        static readonly IList<DlcPatch> communityDlcPatches = new List<DlcPatch> {
            new EditionSensitiveDlcPatch(
                new VersionDescriptionEdition("1.00", "bc40d6195c66c8416c9be007e89a59db4fe568fd",
                    "8856c9df9c7caacb95a5073a6fab5fc7571c8bda"),
                // TODO: We could actually just use 'multiple source checksums' so as array ?
                new VersionDescription("1.07", "818e6332abe89c82ae19a151222e55123bfd3d36")) {
                    ChecksumFile = @"@LIB_DLC_1\addons\if_dlc_mpmissions.pbo"
                },
            new DlcPatch(
                new VersionDescription("1.01", "ca3311650b5be7b44bc1fd245b598c8116654274"),
                new VersionDescription("1.06", "84e1a3d53d75d6cdcb689d597fadd2cc41cf3c07")) {
                    ChecksumFile = @"@LIB_DLC_1\addons\if_dlc_mpmissions.pbo"
                },
            // ARMA2CO only
            new DlcPatch(
                new VersionDescription("1.03", "2dc86bb13c411f054b62fd761cf96a161cfa3f2d"),
                new VersionDescription("1.06", "84e1a3d53d75d6cdcb689d597fadd2cc41cf3c07")) {
                    ChecksumFile = @"@LIB_DLC_1\addons\if_dlc_mpmissions.pbo"
                },
            new DlcPatch(
                new VersionDescription("1.06", "80accb516b890f550372e0b4c77d4117954d875c"),
                dlc107Patch),
            new DlcPatch(
                dlc107Patch,
                new VersionDescription("1.08", "6406e93f669e1642290e5433ed597f223aaad77e"))
        };
        readonly Func<int, IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>>
            _createMirrorSelectorWithLimit;
        readonly Func<IMirrorSelector, ExportLifetimeContext<IMultiMirrorFileDownloader>>
            _createMultiMirrorFileDownloader;
        readonly Func<bool> _isPremium;
        readonly PboTools _pboTools;
        readonly IProcessManager _processManager;

        public IronFrontInstaller(IProcessManager processManager, PboTools pboTools, Func<bool> isPremium,
            Func<IMirrorSelector, ExportLifetimeContext<IMultiMirrorFileDownloader>> createMultiMirrorFileDownloader,
            Func<int, IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>> createMirrorSelectorWithLimit) {
            _processManager = processManager;
            _pboTools = pboTools;
            _isPremium = isPremium;
            _createMultiMirrorFileDownloader = createMultiMirrorFileDownloader;
            _createMirrorSelectorWithLimit = createMirrorSelectorWithLimit;
        }

        // TODO: The actionHandler concept should be refactored out of this domain service...
        public void Install(IronFrontInfo info, Action<StatusRepo, string, Action> actionHandler) {
            Confirmations(info);

            if (info.IsInstalled()) {
                if (!ConfirmPatchedToLatestVersion(info))
                    actionHandler(info.Status, "IFA Patching", () => ApplyLatestPatches(info));
                return;
            }

            actionHandler(info.Status, "IFA Conversion", () => CleanInstall(info));
        }

        void CleanInstall(IronFrontInfo info) {
            InstallOfficialPatches(info);

            ProcessMainIF(info);
            var dlcProcessed = ProcessDlc(info);
            ProcessCommunityGamePatches(info);

            if (dlcProcessed)
                ProcessCommunityDlcPatches(info.IFDlcSourcePath, info);
        }

        void Confirmations(IronFrontInfo message) {
            if (!ConfirmTempSpaceAvailable(message)) {
                throw new TemporaryDriveFullException("Drive Full", RequiredTempFreeSpace,
                    GetDrive(message.TempPath).AvailableFreeSpace, message.TempPath);
            }

            if (!ConfirmDestinationSpaceAvailable(message)) {
                throw new DestinationDriveFullException("Drive Full", RequiredFreeSpace,
                    GetDrive(message.GamePath).AvailableFreeSpace, message.GamePath);
            }

            if (Tools.Processes.Uac.CheckUac())
                throw new ElevationRequiredException();
        }

        public bool ConfirmPatchedToLatestVersion(IronFrontInfo spec) {
            var gamePatched = GetGameLatestPatchedStatus(spec);
            if (!GetDlcInstalledStatus(spec))
                return gamePatched;

            return gamePatched && GetDlcLatestPatchedStatus(spec);
        }

        void ApplyLatestPatches(IronFrontInfo spec) {
            if (!GetGameLatestPatchedStatus(spec))
                HandleGameCommunityPatches(spec);
            if (GetDlcInstalledStatus(spec) && !GetDlcLatestPatchedStatus(spec))
                HandleDlcCommunityPatches(spec);
        }

        void HandleDlcCommunityPatches(IronFrontInfo spec) {
            var dlcEdition = GetDlcType(spec.IFDlcSourcePath);
            var first = GetFirstRequiredDlcPatch(spec, dlcEdition);
            if (first == null)
                throw new UnrecognizedIFVersionException("Unsupported IFDLC version");

            string lastVersion = null;
            foreach (var dlcPatch in communityDlcPatches.Skip(communityDlcPatches.IndexOf(first))) {
                if (lastVersion != null && dlcPatch.From.Version != lastVersion)
                    continue;
                DownloadAndInstallCommunityPatch(spec, dlcPatch.GetFileName(spec.Game, dlcEdition));
                lastVersion = dlcPatch.To.Version;
            }
        }

        static DlcPatch GetFirstRequiredDlcPatch(IronFrontInfo spec, DlcEdition edition) {
            return
                communityDlcPatches.LastOrDefault(
                    dlcPatch => GetPatchStatus(spec, dlcPatch.ChecksumFile, dlcPatch.GetFromChecksum(edition)));
        }

        void HandleGameCommunityPatches(IronFrontInfo spec) {
            var first = GetFirstRequiredGamePatch(spec);
            if (first == null)
                throw new UnrecognizedIFVersionException("Unrecognized IF version");

            string lastVersion = null;
            foreach (var gamePatch in communityGamePatches.Skip(communityGamePatches.IndexOf(first))) {
                if (lastVersion != null && gamePatch.From.Version != lastVersion)
                    continue;
                DownloadAndInstallCommunityPatch(spec, gamePatch.GetFileName(spec.Game));
                lastVersion = gamePatch.To.Version;
            }
        }

        static GamePatch GetFirstRequiredGamePatch(IronFrontInfo spec) {
            return
                communityGamePatches.LastOrDefault(
                    gamePatch => GetPatchStatus(spec, gamePatch.ChecksumFile, gamePatch.From.Checksum));
        }

        static bool GetDlcLatestPatchedStatus(IronFrontInfo spec) {
            var latestDlcPatch = communityDlcPatches.Last();
            return GetPatchStatus(spec, latestDlcPatch.ChecksumFile, latestDlcPatch.To.Checksum);
        }

        static bool GetPatchStatus(IronFrontInfo spec, string file, string sha) {
            return ConfirmChecksum(spec.GamePath.GetChildFileWithName(file), sha);
        }

        static bool ConfirmChecksum(IAbsoluteFilePath filePath, string expectedSha) {
            return filePath.Exists && Tools.HashEncryption.SHA1FileHash(filePath) == expectedSha;
        }

        static bool GetGameLatestPatchedStatus(IronFrontInfo spec) {
            var latestGamePatch = communityGamePatches.Last();
            return GetPatchStatus(spec, latestGamePatch.ChecksumFile, latestGamePatch.To.Checksum);
        }

        void ProcessCommunityGamePatches(IronFrontInfo spec) {
            string lastVersion = null;
            foreach (var gamePatch in communityGamePatches) {
                if (lastVersion != null && gamePatch.From.Version != lastVersion)
                    continue;
                DownloadAndInstallCommunityPatch(spec, gamePatch.GetFileName(spec.Game));
                lastVersion = gamePatch.To.Version;
            }
        }

        void ProcessMainIF(IronFrontInfo spec) {
            Status("main conversion", spec.Status,
                status => ProcessIFDirectory(spec.IFSourcePath, spec.GameIFPath, spec.TempPath, status),
                RepoStatus.Processing);
        }

        bool ProcessDlc(IronFrontInfo spec) {
            var dlcInstalled = GetDlcInstalledStatus(spec);
            if (dlcInstalled) {
                Status("dlc conversion", spec.Status,
                    status => ProcessIFDirectory(spec.IFDlcSourcePath, spec.GameIFDlcPath, spec.TempPath, status),
                    RepoStatus.Processing);
            }
            return dlcInstalled;
        }

        static bool GetDlcInstalledStatus(IronFrontInfo spec) {
            return spec.IFDlcSourcePath.Exists;
        }

        void ProcessIFDirectory(IAbsoluteDirectoryPath source, IAbsoluteDirectoryPath destination,
            IAbsoluteDirectoryPath tempPath, IStatus status) {
            using (new TmpDirectory(tempPath))
                ExtractFolder(source, tempPath, destination, status);
        }

        void ExtractFolder(IAbsoluteDirectoryPath rootPath, IAbsoluteDirectoryPath tempPath,
            IAbsoluteDirectoryPath destination, IStatus status) {
            destination = destination.GetChildDirectoryWithName("addons");
            destination.MakeSurePathExists();
            var files = Directory.GetFiles(Path.Combine(rootPath.ToString(), "addons"), "*.ifa");
            var i = 0;
            foreach (var f in files) {
                ProcessPbo(f, tempPath, destination);
                i++;
                status.Progress = ((double) i/files.Length)*100;
            }
        }

        void ProcessPbo(string pboFile, IAbsoluteDirectoryPath tempPath, IAbsoluteDirectoryPath destination) {
            var d = tempPath.GetChildDirectoryWithName(Path.GetFileNameWithoutExtension(pboFile));
            using (new TmpDirectory(d)) {
                _pboTools.RunExtractPboWithParameters(pboFile.ToAbsoluteFilePath(), tempPath,
                    "RYDPK");
                MakePbo(d, destination);
            }
        }

        void MakePbo(IAbsoluteDirectoryPath directory, IAbsoluteDirectoryPath destination) {
            _pboTools.RunMakePboWithParameters(directory, destination, "ANJUWP");
        }

        void ProcessCommunityDlcPatches(IAbsoluteDirectoryPath dlcPath, IronFrontInfo spec) {
            if (!dlcPath.Exists)
                return;
            var dlcEdition = GetDlcType(spec.IFDlcSourcePath);
            string lastVersion = null;
            foreach (var dlcPatch in communityDlcPatches) {
                if (lastVersion != null && dlcPatch.From.Version != lastVersion)
                    continue;
                DownloadAndInstallCommunityPatch(spec, dlcPatch.GetFileName(spec.Game, dlcEdition));
                lastVersion = dlcPatch.To.Version;
            }
        }

        static DlcEdition GetDlcType(IAbsoluteDirectoryPath dlcPath) {
            var file = Path.Combine(dlcPath.ToString(), "addons", "france_data.ifa").ToAbsoluteFilePath();
            return ConfirmChecksum(file, OfficialGameInfo.DigitalDlcSha)
                ? DlcEdition.Digitial
                : DlcEdition.Steam;
        }

        void DownloadAndInstallCommunityPatch(IronFrontInfo spec, string patchFile) {
            using (new TmpDirectory(spec.TempPath)) {
                Status(patchFile, spec.Status,
                    status => DownloadAndInstallCommunityPatchInternal(patchFile, spec.TempPath, status, spec.GamePath));
            }
        }

        void DownloadAndInstallCommunityPatchInternal(string patchFile, IAbsoluteDirectoryPath destinationPath,
            ITransferStatus status, IAbsoluteDirectoryPath gamePath) {
            var filePath = destinationPath.GetChildFileWithName(patchFile);
            Download(patchFile, status, filePath);
            status.Reset();
            var gameFilePath = gamePath.GetChildFileWithName(filePath.FileName);
            Tools.Compression.Unpack(filePath, gamePath, true, true, true);
            try {
                InstallPatch(gameFilePath, "-silent -incurrentfolder");
            } finally {
                gameFilePath.FileInfo.Delete();
            }
        }

        void Download(string patchFile, ITransferStatus status, IAbsoluteFilePath filePath) {
            using (
                var scoreMirrorSelector = _createMirrorSelectorWithLimit(10,
                    GetMirrors().Select(x => new Uri(x + "/" + CdnPath)).ToArray()))
            using (var multiMirrorFileDownloader = _createMultiMirrorFileDownloader(scoreMirrorSelector.Value)) {
                multiMirrorFileDownloader.Value.Download(new MultiMirrorFileDownloadSpec(patchFile, filePath) {
                    Progress = status
                });
            }
        }

        IEnumerable<string> GetMirrors() {
            return _isPremium()
                ? Common.PremiumMirrors.Concat(Common.DefaultMirrors)
                : Common.DefaultMirrors;
        }

        void InstallOfficialPatches(IronFrontInfo ifaSpec) {
            if (!ifaSpec.IronFrontExePath.Exists)
                throw new UnsupportedIFAVersionException("exe not found");

            var exeVersion = GetExeVersion(ifaSpec.IronFrontExePath);
            if (IsRequiredVersion(exeVersion))
                return;

            if (exeVersion.Major != 1)
                throw new UnsupportedIFAVersionException(exeVersion.ToString());

            switch (exeVersion.Minor) {
            case 60:
                PatchFrom0(ifaSpec);
                break;
            case 63:
                PatchFrom3(ifaSpec);
                break;
            case 64:
                PatchFrom4(ifaSpec);
                break;
            default:
                throw new UnsupportedIFAVersionException(exeVersion.ToString());
            }

            exeVersion = GetExeVersion(ifaSpec.IronFrontExePath);
            if (exeVersion < requiredVersion)
                throw new UnsupportedIFAVersionException(exeVersion.ToString());
        }

        void PatchFrom0(IronFrontInfo ifaSpec) {
            var esd = !ConfirmChecksum(ifaSpec.IronFrontExePath, OfficialGameInfo.Disk0ExeSha);
            InstallOfficialPatch0To3(ifaSpec, esd);
            InstallOfficialPatch3To4(ifaSpec, esd);
            InstallOfficialPatch4To5(ifaSpec, esd);
        }

        void PatchFrom3(IronFrontInfo ifaSpec) {
            var esd = !ConfirmChecksum(ifaSpec.IronFrontExePath, OfficialGameInfo.Disk3ExeSha);
            InstallOfficialPatch3To4(ifaSpec, esd);
            InstallOfficialPatch4To5(ifaSpec, esd);
        }

        void PatchFrom4(IronFrontInfo ifaSpec) {
            var esd = !ConfirmChecksum(ifaSpec.IronFrontExePath, OfficialGameInfo.Disk4ExeSha);
            InstallOfficialPatch4To5(ifaSpec, esd);
        }

        static bool IsRequiredVersion(Version exeVersion) {
            return exeVersion.Major == requiredVersion.Major && exeVersion.Minor == requiredVersion.Minor;
        }

        void InstallOfficialPatch0To3(IronFrontInfo ifaSpec, bool esd) {
            InstallOfficialPatch(esd ? OfficialGameInfo.Patch0To3Esd : OfficialGameInfo.Patch0To3, ifaSpec.TempPath,
                ifaSpec.Status);
        }

        void InstallOfficialPatch3To4(IronFrontInfo ifaSpec, bool esd) {
            InstallOfficialPatch(esd ? OfficialGameInfo.Patch3To4Esd : OfficialGameInfo.Patch3To4, ifaSpec.TempPath,
                ifaSpec.Status);
        }

        void InstallOfficialPatch4To5(IronFrontInfo ifaSpec, bool esd) {
            InstallOfficialPatch(esd ? OfficialGameInfo.Patch4To5Esd : OfficialGameInfo.Patch4To5, ifaSpec.TempPath,
                ifaSpec.Status);
        }

        void InstallOfficialPatch(string patchFile, IAbsoluteDirectoryPath tempPath, StatusRepo repo) {
            using (new TmpDirectory(tempPath))
                Status(patchFile, repo, status => InstallOfficialPatchInternal(patchFile, tempPath, status));
        }

        void InstallOfficialPatchInternal(string patchFile, IAbsoluteDirectoryPath tempPath, ITransferStatus status) {
            var filePath = tempPath.GetChildFileWithName(patchFile);
            Download(patchFile, status, filePath);
            status.Reset();
            InstallPatch(filePath, "-silent");
        }

        static void Status(string patchFile, StatusRepo repo, Action<IStatus> act,
            RepoStatus action = RepoStatus.Downloading) {
            var status = new Status(patchFile, repo) {Action = action};
            act(status);
            status.EndOutput();
        }

        ProcessExitResult InstallPatch(IAbsoluteFilePath filePath, string parameters) {
            return _processManager.Launch(new BasicLaunchInfo(new ProcessStartInfo(filePath.ToString(), parameters)));
        }

        static Version GetExeVersion(IAbsoluteFilePath exePath) {
            return Tools.FileUtil.GetVersion(exePath);
        }

        bool ConfirmSpaceAvailable(IronFrontInfo ifaSpec) {
            return (ConfirmTempSpaceAvailable(ifaSpec) && ConfirmDestinationSpaceAvailable(ifaSpec));
        }

        bool ConfirmTempSpaceAvailable(IronFrontInfo ifaSpec) {
            return GetDrive(ifaSpec.TempPath).AvailableFreeSpace > RequiredTempFreeSpace;
        }

        bool ConfirmDestinationSpaceAvailable(IronFrontInfo ifaspec) {
            return GetDrive(ifaspec.GamePath).AvailableFreeSpace > RequiredFreeSpace;
        }

        static DriveInfo GetDrive(IAbsoluteDirectoryPath path) {
            var lower = path.ToString().ToLower();
            return DriveInfo.GetDrives()
                .Single(d => lower.StartsWith(d.Name.ToLower()));
        }

        enum DlcEdition
        {
            Digitial,
            Steam
        }

        class DlcPatch : Patch
        {
            public DlcPatch(VersionDescription from, VersionDescription to) : base(@from, to) {
                ChecksumFile = @"@LIB_DLC_1\addons\lib_dlc_core.pbo";
            }

            public virtual string GetFromChecksum(DlcEdition edition) {
                return From.Checksum;
            }

            public virtual string GetFileName(IfaGameEdition game, DlcEdition edition) {
                return "IronFront_DLC_" + game + "_CommunityPatch_" + From.Version + "-" + To.Version + ".exe";
            }
        }

        class EditionSensitiveDlcPatch : DlcPatch
        {
            readonly VersionDescriptionEdition _from;

            public EditionSensitiveDlcPatch(VersionDescriptionEdition from, VersionDescription to) : base(@from, to) {
                _from = from;
            }

            public override string GetFromChecksum(DlcEdition edition) {
                return edition == DlcEdition.Digitial ? _from.ChecksumDigital : _from.Checksum;
            }

            public override string GetFileName(IfaGameEdition game, DlcEdition edition) {
                return "IronFront_DLC_" + game + "_CommunityPatch_" + From.Version + "-" + To.Version + "_" + edition +
                       ".exe";
            }
        }

        class GamePatch : Patch
        {
            public GamePatch(VersionDescription from, VersionDescription to) : base(@from, to) {
                ChecksumFile = @"@IF\Addons\lib_core.pbo";
            }

            public string GetFileName(IfaGameEdition game) {
                return "IronFront_" + game + "_CommunityPatch_" + From.Version + "-" + To.Version + ".exe";
            }
        }

        // TODO: Consider to convert also to the new patch system?
        static class OfficialGameInfo
        {
            public const string Patch0To3 = "IF44Patch_1.00-1.03_DISK.exe";
            public const string Patch3To4 = "IFPatch_1.03-1.04_Disc.exe";
            public const string Patch4To5 = "IFPatch_1.04-1.05_disc.exe";
            public const string Patch0To3Esd = "IF44Patch_1.00-1.03_ESD.exe";
            public const string Patch3To4Esd = "IFPatch_1.03-1.04_ESD.exe";
            public const string Patch4To5Esd = "IFPatch_1.04-1.05_esd.exe";
            public const string DigitalDlcSha = "c9ff8ac4b1956be25b613d3ba6091cd651fef4c3";
            public const string Disk0ExeSha = "46ab69bd0868e5f313c00ce3c2c38fd29b968856";
            public const string Disk3ExeSha = "e94b7bd42fac7f7a01b6ea69641b79f8d917f178";
            public const string Disk4ExeSha = "48bd55141b7ddd5d471a274e1528a8299696610d";
        }

        abstract class Patch
        {
            protected Patch(VersionDescription from, VersionDescription to) {
                From = @from;
                To = to;
            }

            public VersionDescription From { get; }
            public VersionDescription To { get; }
            public string ChecksumFile { get; set; }
        }

        class UnrecognizedIFVersionException : Exception
        {
            public UnrecognizedIFVersionException(string message) : base(message) {}
        }

        class VersionDescription
        {
            public VersionDescription(string version, string checksum) {
                Version = version;
                Checksum = checksum;
            }

            public string Version { get; }
            public string Checksum { get; }
        }

        class VersionDescriptionEdition : VersionDescription
        {
            public VersionDescriptionEdition(string version, string steamChecksum, string digitalChecksum)
                : base(version, steamChecksum) {
                ChecksumDigital = digitalChecksum;
            }

            public string ChecksumDigital { get; }
        }
    }

    public enum IfaGameEdition
    {
        Arma2CO,
        Arma3
    }

    public class TemporaryDriveFullException : Exception
    {
        public TemporaryDriveFullException(string message, long requiredSpace, long availableSpace,
            IAbsoluteDirectoryPath path)
            : base(message, new IOException(message, WindowsAPIErrorCodes.ERROR_DISK_FULL)) {
            RequiredSpace = requiredSpace;
            AvailableSpace = availableSpace;
            Path = path;
        }

        public TemporaryDriveFullException(string message, long requiredSpace, long availableSpace,
            IAbsoluteDirectoryPath path,
            Exception innerException)
            : base(message, innerException) {
            RequiredSpace = requiredSpace;
            AvailableSpace = availableSpace;
            Path = path;
        }

        public long RequiredSpace { get; private set; }
        public long AvailableSpace { get; private set; }
        public IAbsoluteDirectoryPath Path { get; private set; }
    }

    public class ElevationRequiredException : WindowsAPIExcpetion
    {
        public ElevationRequiredException() : base(WindowsAPIErrorCodes.ERROR_ELEVATION_REQUIRED) {}
    }

    public class IfaStatus : StatusRepo
    {
        public IfaStatus() {
            Action = RepoStatus.Processing;
        }
    }

    public class UnsupportedIFAVersionException : Exception
    {
        public UnsupportedIFAVersionException(string message) : base(message) {}
    }

    public class DestinationDriveFullException : Exception
    {
        public DestinationDriveFullException(string message, long requiredSpace, long availableSpace,
            IAbsoluteDirectoryPath path)
            : base(message, new IOException(message, WindowsAPIErrorCodes.ERROR_DISK_FULL)) {
            RequiredSpace = requiredSpace;
            AvailableSpace = availableSpace;
            Path = path;
        }

        public DestinationDriveFullException(string message, long requiredSpace, long availableSpace,
            IAbsoluteDirectoryPath path,
            Exception innerException)
            : base(message, innerException) {
            RequiredSpace = requiredSpace;
            AvailableSpace = availableSpace;
            Path = path;
        }

        public long RequiredSpace { get; private set; }
        public long AvailableSpace { get; private set; }
        public IAbsoluteDirectoryPath Path { get; private set; }
    }

    public class IronFrontInfo
    {
        public IronFrontInfo(IAbsoluteDirectoryPath ironFrontPath, IAbsoluteDirectoryPath gamePath,
            IAbsoluteDirectoryPath tempPath, IfaStatus status, IfaGameEdition game) {
            IronFrontPath = ironFrontPath;
            GamePath = gamePath;
            TempPath = tempPath;

            Status = status;
            Game = game;

            IronFrontExePath = ironFrontPath.GetChildFileWithName("ironfront.exe");
            IFSourcePath = ironFrontPath.GetChildDirectoryWithName("IF");
            IFDlcSourcePath = ironFrontPath.GetChildDirectoryWithName("DLC_1");
            GameIFPath = gamePath.GetChildDirectoryWithName("@IF");
            GameIFDlcPath = gamePath.GetChildDirectoryWithName("@LIB_DLC_1");
            GameIF3MPath = gamePath.GetChildDirectoryWithName("@IFA3M");
            GameIFOtherAddonsPath = gamePath.GetChildDirectoryWithName("@IF_Other_Addons");
        }

        public IfaGameEdition Game { get; set; }
        public IAbsoluteDirectoryPath IFDlcSourcePath { get; }
        public IAbsoluteDirectoryPath IFSourcePath { get; }
        public IAbsoluteDirectoryPath GameIFDlcPath { get; }
        public IAbsoluteDirectoryPath GameIFPath { get; }
        public IAbsoluteDirectoryPath GameIFOtherAddonsPath { get; }
        public IAbsoluteDirectoryPath GameIF3MPath { get; }
        public IfaStatus Status { get; }
        public IAbsoluteDirectoryPath TempPath { get; }
        public IAbsoluteDirectoryPath GamePath { get; }
        public IAbsoluteDirectoryPath IronFrontPath { get; private set; }
        public IAbsoluteFilePath IronFrontExePath { get; }

        public bool IsInstalled() {
            return GameIFPath.Exists && GameIF3MPath.Exists &&
                   GameIFOtherAddonsPath.Exists
                   && (!IFDlcSourcePath.Exists || GameIFDlcPath.Exists);
        }
    }
}