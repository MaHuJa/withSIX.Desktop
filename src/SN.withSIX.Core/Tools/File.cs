// <copyright company="SIX Networks GmbH" file="File.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Core.Validators;

namespace SN.withSIX.Core
{
    public static partial class Tools
    {
        public static FileTools FileUtil = new FileTools();

        public partial class FileTools : IEnableLogging
        {
            public enum Units
            {
                B = 0,
                kB,
                MB,
                GB,
                TB,
                PB,
                EB,
                ZB,
                YB
            }

            static readonly Regex sizeA = new Regex(@"\.pbo(\.gz)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            static readonly Regex sizeB = new Regex(@"(\.bisign|\.bikey)(\.gz)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            static readonly Regex sizeC =
                new Regex(@"\.(txt|doc|cpp|hpp|sqf|sqs|fsm|html?|jpg|png|ico|exe|dll)(\.gz)?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static bool IsRunningOnMono = GetIsRunningOnMono();
            // TODO: Switch over to custom combine path method due to problematic behavior of built in Path.Combine method
            // had error reports come in from someone using 'E:' as mod/synq path, generating paths like E:@dayz\filename, of which the app thinks are valid...)
            // http://thingsihateaboutmicrosoft.blogspot.nl/2009/08/pathcombine-is-essentially-useless.html
            // TODO: Also input fields should probably validate to confirm there are no relative paths used, only absolute paths
            /*
            public static string CombinePath(params string[] paths) {
                // TODO: HOWEVER MAYBE ITS A GOOD IDEA TO SEE IF WE CAN FIX THE INPUT PATHS, OR THROW EXCEPTIONS WHEN WE FIND PATHS ARE WRONG INSTEAD!
                // So CheckPath could check for it and throw, as well as the combiner
                return string.Join(Path.DirectorySeparatorChar.ToString(), paths.Select(x => x.Trim('\\', '/')));
            }
             */

            public readonly FileOps Ops = new FileOps();

            public bool ComparePathsOsCaseSensitive(string path1, string path2) {
                return GetFullCleanPath(path1).Equals(GetFullCleanPath(path2));
            }

            public bool ComparePathsOsCaseSensitive(IAbsolutePath path1, IAbsolutePath path2) {
                return GetFullCleanPath(path1.ToString()).Equals(GetFullCleanPath(path2.ToString()));
            }

            public bool ComparePathsEqualCase(string source, string destination) {
                return Path.GetFullPath(source.CleanPath()).Equals(Path.GetFullPath(destination.CleanPath()));
            }

            // TODO: Deal in IAbsoluteFilePaths instead?
            public IEnumerable<FileInfo> GetFiles(IAbsoluteDirectoryPath directory, string fileMask = "*.*",
                IEnumerable<string> dirRootExclusions = null, IEnumerable<string> fileRootExclusions = null) {
                var di = directory.DirectoryInfo;
                var dirs = di.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly);
                if (dirRootExclusions != null) {
                    dirs =
                        dirs.Where(x => !dirRootExclusions.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase));
                }
                var files = di.EnumerateFiles(fileMask, SearchOption.TopDirectoryOnly);
                if (fileRootExclusions != null) {
                    files =
                        files.Where(x => !fileRootExclusions.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase));
                }
                return files
                    .Concat(dirs.SelectMany(x => x.EnumerateFiles(fileMask, SearchOption.AllDirectories)));
            }

            public string FindPathInParents(string path, string key) {
                var parent = Directory.GetParent(path);
                while (parent != null) {
                    var dir = Path.Combine(parent.FullName, key);
                    if (Directory.Exists(dir))
                        return dir;
                    parent = parent.Parent;
                }
                return null;
            }

            public async Task Lock(string path, Func<Task> action) {
                using (var autoResetEvent = new AutoResetEvent(false)) {
                    try {
                        using (var fileSystemWatcher =
                            new FileSystemWatcher(Path.GetDirectoryName(path)) {
                                EnableRaisingEvents = true
                            }) {
                            fileSystemWatcher.Deleted +=
                                (o, e) => {
                                    if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(path))
                                        autoResetEvent.Set();
                                };

                            while (true) {
                                try {
                                    using (var file = File.Open(path,
                                        FileMode.OpenOrCreate,
                                        FileAccess.ReadWrite,
                                        FileShare.None)) {
                                        fileSystemWatcher.Dispose();
                                        autoResetEvent.Dispose();
                                        await action().ConfigureAwait(false);
                                        break;
                                    }
                                } catch (IOException) {
                                    autoResetEvent.WaitOne();
                                    autoResetEvent.Reset();
                                }
                            }
                        }
                    } finally {
                        File.Delete(path);
                    }
                }
            }

            public string FindParentWithName(string path, string key) {
                var parent = Directory.GetParent(path);
                while (parent != null) {
                    if (parent.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        return parent.FullName;
                    parent = parent.Parent;
                }
                return null;
            }

            public bool IsPathRootedIn(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath root, bool mayBeEqual = false) {
                if (ComparePathsOsCaseSensitive(path, root))
                    return mayBeEqual;

                var rt = root.ToString();
                var dir = path;
                while (dir != null && dir.HasParentDirectory) {
                    var parent = dir.ParentDirectoryPath;
                    if (ComparePathsOsCaseSensitive(parent.ToString(), rt))
                        return true;
                    dir = parent;
                }
                return false;
            }

            public bool IsPathRootedDirectlyIn(IAbsoluteDirectoryPath path, IAbsoluteDirectoryPath root,
                bool mayBeEqual = false) {
                if (ComparePathsOsCaseSensitive(path, root))
                    return mayBeEqual;
                return path.HasParentDirectory &&
                       ComparePathsOsCaseSensitive(path.ParentDirectoryPath.ToString(), root.ToString());
            }

            public string GetFileSize(double size, Units unit = Units.B, string postFix = null) {
                if (size < 0)
                    return DefaultSizeReturn;

                while (size >= 1024) {
                    size /= 1024;
                    ++unit;
                }

                var s = String.Format("{0:0.##} {1}", size, unit);
                return postFix == null ? s : s + postFix;
            }

            string GetFullCleanPath(string path) {
                return CleanPath(Path.GetFullPath(path.EndsWith(":") ? path + "\\" : path));
            }

            [Obsolete("Running on Mono does not have to Equal file system is case sensitive :S")]
            public string CleanPath(string path) {
                path = path.CleanPath();
                return IsRunningOnMono ? path : path.ToLower();
            }

            static bool GetIsRunningOnMono() {
                return Type.GetType("Mono.Runtime") != null;
            }

            static bool IsSameRoot(IAbsoluteDirectoryPath sourcePath, IAbsoluteDirectoryPath destinationPath) {
                return Directory.GetDirectoryRoot(sourcePath.ToString()) ==
                       Directory.GetDirectoryRoot(destinationPath.ToString());
            }

            public bool IsValidRootedPath(string path, bool checkExists = false, bool mayBeUnc = false) {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                try {
                    if (!path.IsValidAbsoluteDirectoryPath())
                        return false;
                    var absolutePath = path.ToAbsoluteDirectoryPath();
                    PathValidator.ValidateName(path);
                    if (!mayBeUnc && absolutePath.Kind == AbsolutePathKind.UNC)
                        return false;
                    return !checkExists || absolutePath.Exists;
                } catch (ArgumentException) {
                    return false;
                } catch (PathTooLongException) {
                    return false;
                } catch (NotSupportedException) {
                    return false;
                }
            }

            public bool CheckFile(string path, bool checkExists = false, bool mayBeUnc = true) {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                try {
                    PathValidator.ValidateName(path);
                    var existsCheck = !checkExists || File.Exists(path);
                    var uncCheck = mayBeUnc || !(new FileInfo(path).IsUncPath());
                    return existsCheck && uncCheck;
                } catch (ArgumentException) {
                    return false;
                }
            }

            public void OpenFolderInExplorer(string path) {
                Contract.Requires<ArgumentNullException>(path != null);
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(path));

                ProcessManager.StartAndForget(new ProcessStartInfo("Explorer.exe", "\"" + path + "\""));
            }

            public void OpenFolderInExplorer(IAbsoluteDirectoryPath path) {
                Contract.Requires<ArgumentNullException>(path != null);

                ProcessManager.StartAndForget(new ProcessStartInfo("Explorer.exe", "\"" + path + "\""));
            }

            public Version GetVersion(IAbsoluteFilePath exePath) {
                Contract.Requires<ArgumentNullException>(exePath != null);

                if (!exePath.Exists)
                    return null;

                return FileVersionInfo.GetVersionInfo(exePath.ToString()).ProductVersion.TryParseVersion();
            }

            public FileVersionInfo GetFileVersion(string file) {
                Contract.Requires<ArgumentNullException>(file != null);
                Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(file));

                if (!File.Exists(file))
                    return null;

                return FileVersionInfo.GetVersionInfo(file);
            }

            static string MakeValidShortcutFileName(string fileName) {
                return MakeValidFileName(fileName, ".lnk");
            }

            static string MakeValidBatFileName(string fileName) {
                return MakeValidFileName(fileName, ".bat");
            }

            static string MakeValidFileName(string fileName, string ext) {
                return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()).Take(255 - ext.Length)) + ext;
            }

            public Task CreateShortcutAsync(ShortcutInfo info) {
                return Task.Run(() => CreateShortcut(info));
            }

            public Task CreateBatFile(IAbsoluteDirectoryPath path, string name, string content) {
                return
                    Ops.CreateTextAsync(
                        path.GetChildFileWithName(MakeValidBatFileName(name)), content);
            }

            public void CreateShortcut(ShortcutInfo info) {
                var sanitizedLinkFile = info.DestinationPath.GetChildFileWithName(MakeValidShortcutFileName(info.Name));
                var shortcut = new ShellLink {
                    Target = info.Target.ToString(),
                    Arguments = info.Arguments,
                    WorkingDirectory = info.WorkingDirectory == null ? null : info.WorkingDirectory.ToString(),
                    Description = info.Description,
                    IconPath = info.Icon == null ? null : info.Icon.ToString()
                };
                shortcut.Save(sanitizedLinkFile.ToString());
            }

            public Icon AddIconOverlay(Icon originalIcon, Icon overlay) {
                Image a = originalIcon.ToBitmap();
                Image b = overlay.ToBitmap();
                var bitmap = new Bitmap(a.Width, a.Height);
                var canvas = Graphics.FromImage(bitmap);
                canvas.DrawImage(a, new Point(0, 0));
                canvas.DrawImage(b, new Point(16, 16));
                canvas.Save();
                return Icon.FromHandle(bitmap.GetHicon());
            }

            public void SelectInExplorer(string path) {
                Contract.Requires<ArgumentNullException>(path != null);
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(path));

                ProcessManager.StartAndForget(new ProcessStartInfo("Explorer.exe",
                    String.Format("/select,\"{0}\"", path)));
            }

            public long SizePrediction(string filePath) {
                Contract.Requires<ArgumentNullException>(filePath != null);

                var fileName = Path.GetFileName(filePath);
                if (sizeA.IsMatch(fileName))
                    return 2*FileSizeUnits.MB;
                if (sizeB.IsMatch(fileName))
                    return FileSizeUnits.KB;
                if (sizeC.IsMatch(fileName))
                    return 2*FileSizeUnits.KB;

                return FileSizeUnits.MB;
            }

            public string[] OrderBySize(IEnumerable<string> ar, bool reverse = false) {
                var ordered = ar.OrderBy(SizePrediction);
                return reverse ? ordered.Reverse().ToArray() : ordered.ToArray();
            }

            public string RemoveExtension(string filePath, string toRemove = null) {
                var extension = Path.GetExtension(filePath);
                if (extension == null
                    || toRemove == null
                    || !extension.Equals(toRemove, StringComparison.OrdinalIgnoreCase))
                    return filePath;
                return filePath.Substring(0, filePath.Length - extension.Length);
            }

            public long GetDirectorySize(IAbsoluteDirectoryPath p, string filter = null, string selector = "*.*",
                bool recurse = true) {
                var sum =
                    Directory.EnumerateFiles(p.ToString(), selector,
                        recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .Select(name => new FileInfo(name).Length)
                        .Sum();

                if (filter != null) {
                    sum = sum -
                          Directory.EnumerateFiles(p.ToString(), filter,
                              recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                              .Select(name => new FileInfo(name).Length)
                              .Sum();
                }

                return sum;
            }

            public string FilterPath(string pathName) {
                if (string.IsNullOrWhiteSpace(pathName))
                    return pathName;

                return GenericTools.PathFilter.Replace(pathName,
                    match1 => match1.Groups[1].Value + new string('*', match1.Groups[2].Length) + match1.Groups[3].Value);
            }

            public string FilterPath(FileSystemInfo info) {
                return info == null ? null : FilterPath(info.FullName);
            }

            public string FilterPath(IPath info) {
                return info == null ? null : FilterPath(info.ToString());
            }

            public void HandleDowncase(string entry, string[] excludes = null) {
                if (Directory.Exists(entry))
                    HandleDowncaseFolder(entry.ToAbsoluteDirectoryPath(), excludes);

                Ops.DowncasePath(entry);
            }

            public void HandleDowncaseFolder(IAbsoluteDirectoryPath entry, string[] excludes = null) {
                if (excludes == null)
                    excludes = new string[0];

                foreach (var file in Directory.EnumerateFiles(entry.ToString(), "*.*", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.EnumerateDirectories(entry.ToString(), "*.*",
                        SearchOption.TopDirectoryOnly))) {
                    if (excludes.Any(x => x == Path.GetFileName(file)))
                        continue;
                    HandleDowncase(file, excludes);
                }
            }

            public bool IsDirectoryEmpty(IAbsoluteDirectoryPath path) {
                return !Directory.EnumerateFileSystemEntries(path.ToString()).Any();
            }

            public void OpenCommandPrompt(string path) {
                var startInfo =
                    new ProcessStartInfo(Common.Paths.CmdExe.ToString(), null).SetWorkingDirectoryOrDefault(path);
                ProcessManager.Launch(new BasicLaunchInfo(startInfo));
            }

            [Flags]
            internal enum MoveFileFlags
            {
                None = 0,
                ReplaceExisting = 1,
                CopyAllowed = 2,
                DelayUntilReboot = 4,
                WriteThrough = 8,
                CreateHardlink = 16,
                FailIfNotTrackable = 32
            }

            static class NativeMethods
            {
                [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                public static extern bool MoveFileEx(
                    string lpExistingFileName,
                    string lpNewFileName,
                    MoveFileFlags dwFlags);
            }
        }
    }

    public class ShortcutInfo
    {
        public ShortcutInfo(IAbsoluteDirectoryPath destinationPath, string name, IAbsoluteFilePath target) {
            Contract.Requires<ArgumentNullException>(destinationPath != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(target != null);
            DestinationPath = destinationPath;
            Name = name;
            Target = target;
        }

        public IAbsoluteDirectoryPath DestinationPath { get; }
        public IAbsoluteFilePath Target { get; }
        public string Name { get; }
        public string Description { get; set; }
        public IAbsoluteFilePath Icon { get; set; }
        public IAbsoluteDirectoryPath WorkingDirectory { get; set; }
        public string Arguments { get; set; }
    }

    public static class FileSizeUnits
    {
        const int Unit = 1024;
        public const int KB = Unit;
        public const int MB = KB*Unit;
        public const long GB = MB*Unit;
        public const long TB = GB*Unit;
    }

    public static class PathExtensions
    {
        public static void MakeSurePathExists(this string path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Tools.FileUtil.Ops.CreateDirectory(path);
        }

        public static void MakeSurePathExistsWithRetry(this string path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Tools.FileUtil.Ops.CreateDirectoryWithRetry(path);
        }

        public static void MakeSureParentPathExists(this string subPath, bool retry = true) {
            Contract.Requires<ArgumentNullException>(subPath != null);
            var folder = Path.GetDirectoryName(subPath);
            if (!string.IsNullOrWhiteSpace(folder))
                MakeSurePathExists(folder);
        }

        public static void MakeSurePathExists(this IDirectoryPath path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Tools.FileUtil.Ops.CreateDirectory(path.ToString());
        }

        public static void MakeSurePathExistsWithRetry(this IDirectoryPath path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Tools.FileUtil.Ops.CreateDirectoryWithRetry(path.ToString());
        }

        public static void MakeSureParentPathExists(this IDirectoryPath subPath, bool retry = true) {
            Contract.Requires<ArgumentNullException>(subPath != null);
            var folder = subPath.ParentDirectoryPath;
            if (folder != null)
                MakeSurePathExists(folder);
        }

        public static void MakeSureParentPathExists(this IFilePath subPath, bool retry = true) {
            Contract.Requires<ArgumentNullException>(subPath != null);
            var folder = subPath.ParentDirectoryPath;
            if (folder != null)
                MakeSurePathExists(folder);
        }

        public static void RemoveReadonlyWhenExists(this string path) {
            Contract.Requires<ArgumentNullException>(path != null);
            if (File.Exists(path))
                new FileInfo(path).RemoveReadonlyWhenExists();
            else if (Directory.Exists(path))
                new DirectoryInfo(path).RemoveReadonlyWhenExists();
        }

        public static void RemoveReadonlyWhenExists(this FileSystemInfo path) {
            Contract.Requires<ArgumentNullException>(path != null);
            try {
                if (path.Exists && path.Attributes.HasFlag(FileAttributes.ReadOnly))
                    path.Attributes &= ~FileAttributes.ReadOnly;
            } catch (ArgumentException e) {
                throw new UnauthorizedAccessException("Access to '" + path.FullName + "' is denied", e);
            }
        }

        public static void RemoveReadonlyWhenExists(this IAbsoluteFilePath path) {
            Contract.Requires<ArgumentNullException>(path != null);
            if (path.Exists)
                path.FileInfo.RemoveReadonlyWhenExists();
        }

        public static void MakeSurePathExists(this DirectoryInfo path) {
            Contract.Requires<ArgumentNullException>(path != null);

            path.FullName.MakeSurePathExists();
        }

        public static void MakeSurePathExistsWithRetry(this DirectoryInfo path) {
            Contract.Requires<ArgumentNullException>(path != null);
            path.FullName.MakeSurePathExistsWithRetry();
        }

        public static void MakeSureParentPathExists(this DirectoryInfo subPath, bool retry = true) {
            Contract.Requires<ArgumentNullException>(subPath != null);
            var folder = subPath.Parent;
            if (folder != null)
                folder.MakeSurePathExists();
        }
    }
}