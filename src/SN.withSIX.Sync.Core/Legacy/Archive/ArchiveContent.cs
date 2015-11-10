// <copyright company="SIX Networks GmbH" file="ArchiveContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Legacy.Archive
{
    public class ArchiveContent
    {
        const string RepoFolder = ".sixarchive";
        const string ArchiveExtension = ".archive";
        static ArchiveContentFactory _factory;
        static readonly string[] interestFolders = {"addons", "dta", "keys", "userconfig"};
        static readonly string[] docFileExtensions = {
            ".txt", ".md", ".doc", ".pdf", ".log", ".png", ".jpg", ".gif",
            ".ico"
        };
        readonly IFileDownloader _downloader;

        public ArchiveContent(string name, string destination, IFileDownloader downloader) {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(destination != null);
            Name = name;
            Destination = System.IO.Path.GetFullPath(destination);
            _downloader = downloader;

            Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Destination, Name));
            RepoPath = System.IO.Path.Combine(Path, RepoFolder);
            RepoTmpPath = System.IO.Path.Combine(RepoPath, "tmp").ToAbsoluteDirectoryPath();
            ArchivePath = System.IO.Path.Combine(RepoPath, Name + ArchiveExtension).ToAbsoluteFilePath();
        }

        public ArchiveContent(string name, DirectoryInfo destination, IFileDownloader downloader)
            : this(name, destination.FullName, downloader) {}

        public static ArchiveContentFactory Factory
        {
            get { return _factory ?? (_factory = new ArchiveContentFactory()); }
        }
        public IAbsoluteFilePath ArchivePath { get; }
        public string Name { get; }
        public string Destination { get; }
        public string Path { get; }
        public string RepoPath { get; }
        public IAbsoluteDirectoryPath RepoTmpPath { get; }

        public void Create() {
            RepoPath.MakeSurePathExists();
        }

        public void Clean() {
            var dir = new DirectoryInfo(Path);
            foreach (var subDir in dir.EnumerateDirectories()
                .Where(subDir => subDir.Name != RepoFolder
                                 && !subDir.Name.StartsWith(".")))
                Tools.FileUtil.Ops.DeleteFileSystemInfo(subDir);

            foreach (var file in dir.EnumerateFiles())
                Tools.FileUtil.Ops.DeleteFileSystemInfo(file);
        }

        public void Download(Uri url) {
            _downloader.Download(url, ArchivePath);
        }

        public void Download(string url) {
            Contract.Requires<ArgumentNullException>(url != null);
            Download(new Uri(url));
        }

        public Task DownloadAsync(Uri url) {
            return _downloader.DownloadAsync(url, ArchivePath);
        }

        public Task DownloadAsync(string url) {
            Contract.Requires<ArgumentNullException>(url != null);
            return DownloadAsync(new Uri(url));
        }

        public void Update(Uri uri) {
            Download(uri);
            UpdateContent();
        }

        public void Update(string url) {
            Download(url);
            UpdateContent();
        }

        public void UpdateContent() {
            Unpack();
            Clean();
            Process();
        }

        public void Unpack() {
            if (RepoTmpPath.Exists)
                Directory.Delete(RepoTmpPath.ToString(), true);
            RepoTmpPath.MakeSurePathExists();
            Tools.Compression.Unpack(ArchivePath, RepoTmpPath, true);
        }

        public void Process() {
            RecurseDirectory(RepoTmpPath.DirectoryInfo);
            ProcessKeys();

            var addonsPath = System.IO.Path.Combine(Path, "addons");
            var missionsPath = System.IO.Path.Combine(Path, "missions");
            ProcessPbos(missionsPath, addonsPath);
            ProcessBiSigns(addonsPath);
        }

        void ProcessBiSigns(string addonsPath) {
            foreach (var fi in RepoTmpPath.DirectoryInfo.EnumerateFiles("*.bisign", SearchOption.AllDirectories)) {
                addonsPath.MakeSurePathExists();
                Tools.FileUtil.Ops.MoveWithRetry(fi.FullName.ToAbsoluteFilePath(),
                    System.IO.Path.Combine(addonsPath, fi.Name).ToAbsoluteFilePath());
            }
        }

        void ProcessPbos(string missionsPath, string addonsPath) {
            foreach (var fi in RepoTmpPath.DirectoryInfo.EnumerateFiles("*.pbo", SearchOption.AllDirectories)) {
                if (System.IO.Path.GetFileNameWithoutExtension(fi.Name).Contains(".")) {
                    missionsPath.MakeSurePathExists();
                    Tools.FileUtil.Ops.MoveWithRetry(fi.FullName.ToAbsoluteFilePath(),
                        System.IO.Path.Combine(missionsPath, fi.Name).ToAbsoluteFilePath());
                } else {
                    addonsPath.MakeSurePathExists();
                    Tools.FileUtil.Ops.MoveWithRetry(fi.FullName.ToAbsoluteFilePath(),
                        System.IO.Path.Combine(addonsPath, fi.Name).ToAbsoluteFilePath());
                }
            }
        }

        void ProcessKeys() {
            var keyPath = System.IO.Path.Combine(Path, "keys");
            foreach (var fi in RepoTmpPath.DirectoryInfo.EnumerateFiles("*.bikey", SearchOption.AllDirectories)) {
                keyPath.MakeSurePathExists();
                Tools.FileUtil.Ops.MoveWithRetry(fi.FullName.ToAbsoluteFilePath(),
                    System.IO.Path.Combine(keyPath, fi.Name).ToAbsoluteFilePath());
            }
        }

        public void ImportFromArchive(IAbsoluteFilePath file) {
            Create();
            Tools.FileUtil.Ops.CopyWithRetry(file, ArchivePath, true);
            UpdateContent();
        }

        public void ImportFromDownload(string url) {
            Create();
            Download(url);
            UpdateContent();
        }

        public async Task ImportFromDownloadAsync(string url) {
            Create();
            await DownloadAsync(url);
            UpdateContent();
        }

        void RecurseDirectory(DirectoryInfo dir) {
            if (interestFolders.Any(x => dir.Name.Equals(x, StringComparison.OrdinalIgnoreCase))) {
                MoveRootDirectory(dir);
                return;
            }

            foreach (var subDir in dir.EnumerateDirectories())
                RecurseDirectory(subDir);

            foreach (var file in dir.EnumerateFiles()
                .Where(
                    file =>
                        docFileExtensions.Any(
                            x => file.Extension.Equals(x, StringComparison.OrdinalIgnoreCase))))
                MoveRootFile(file);
        }

        void MoveRootDirectory(DirectoryInfo dir) {
            Tools.FileUtil.Ops.MoveDirectory(dir.FullName.ToAbsoluteDirectoryPath(),
                System.IO.Path.Combine(Path, dir.Name).ToAbsoluteDirectoryPath());
        }

        void MoveRootFile(FileSystemInfo file) {
            Tools.FileUtil.Ops.MoveWithRetry(file.FullName.ToAbsoluteFilePath(),
                System.IO.Path.Combine(Path, file.Name).ToAbsoluteFilePath());
        }
    }

    public class ArchiveContentFactory
    {
        static readonly char[] trimStartChars = {'_', '@'};
        static readonly char[] trimEndChars = {'_', '-', '.'};
        static readonly Dictionary<string, string> replacements = new Dictionary<string, string> {
            {"arma3alpha", "a3a"},
            {"arma3", "a3"},
            {"arma2oa", "a2oa"},
            {"arma2co", "a2co"},
            {"arma2", "a2"}
        };
        static readonly Regex RX_MOD = new Regex(@"[_\-\.]+((final|fixed|new|alpha|beta|hotfix|bugfix|fix)[_\-\.]*\d*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex RX_V1 = new Regex(@"^([a-zA-Z_]+?[^v\d])([\d\.]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex RX_V2 =
            new Regex(
                @"^([\w_\-]+?)(([_\-\.]*v[a-zA-Z]?[\d\.]*[a-zA-Z]*|_\d+[\.\d]*[a-zA-Z]*|-[-\d]+[a-zA-Z]*)([_\-\.]*[a-zA-Z]*)?$|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ArchiveContent CreateFromExisting(string name, string destination, IAbsoluteFilePath file,
            IFileDownloader downloader) {
            var ac = new ArchiveContent(name, destination, downloader);
            ac.ImportFromArchive(file);
            return ac;
        }

        public ArchiveContent CreateFromExisting(IAbsoluteDirectoryPath destination, IAbsoluteFilePath file,
            IFileDownloader downloader) {
            var name = GetName(file.ToString());

            var ac = new ArchiveContent(name, destination.ToString(), downloader);
            ac.ImportFromArchive(file);
            return ac;
        }

        public ArchiveContent CreateFromDownload(string name, string destination, string url, IFileDownloader downloader) {
            var ac = new ArchiveContent(name, destination, downloader);
            ac.ImportFromDownload(url);
            return ac;
        }

        public string GetName(string name, bool trimExtension = true) {
            var n = (trimExtension ? Path.GetFileNameWithoutExtension(name) : name).Replace(" ", "_").ToLower();
            n = n.TrimStart(trimStartChars);
            n = n.TrimEnd(trimEndChars);

            if (n.StartsWith("dayz", StringComparison.InvariantCultureIgnoreCase)) {
                if (!n.Contains("dayz_"))
                    n = n.Replace("dayz", "dayz_");
            }

            var version = string.Empty;
            var versionInfo = new List<string>();

            n = replacements.Aggregate(n, (current, r) => current.Replace(r.Key, r.Value));

            n = RX_MOD.Replace(n, match1 => {
                versionInfo.Add(match1.Groups[1].Value);
                return String.Empty;
            });

            n = n.TrimEnd(trimEndChars);

            var match = RX_V1.Match(n);
            if (match.Success) {
                n = match.Groups[1].Value;
                version = match.Groups[2].Value;
                n = n.TrimEnd(trimEndChars);
            } else {
                match = RX_V2.Match(n);
                if (match.Success) {
                    n = match.Groups[1].Value + match.Groups[4].Value;
                    version = match.Groups[2].Value;
                    n = n.TrimEnd(trimEndChars);
                }
            }

            return n;
        }

        public ArchiveContent CreateFromDownload(string destination, string url, IFileDownloader downloader) {
            var name = GetName(url.Split('/').Last());

            var ac = new ArchiveContent(name, destination, downloader);
            ac.ImportFromDownload(url);
            return ac;
        }
    }
}