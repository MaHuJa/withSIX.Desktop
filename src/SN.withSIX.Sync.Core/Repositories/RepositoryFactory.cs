// <copyright company="SIX Networks GmbH" file="RepositoryFactory.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Repositories.Internals;
using Timer = System.Timers.Timer;

namespace SN.withSIX.Sync.Core.Repositories
{
    public class SynqPathException : Exception
    {
        public SynqPathException(string msg) : base(msg) {}
    }

    public class RepositoryFactory
    {
        public Repository Init(IAbsoluteDirectoryPath directory, RepositoryOperationMode? mode = null) {
            Contract.Requires<ArgumentNullException>(directory != null);

            if (File.Exists(directory.ToString()))
                throw new SynqPathException("Already exists file with same name");

            ConfirmEmpty(directory);

            directory.MakeSurePathExists();
            var repo = new Repository(directory, true);
            if (mode != null)
                repo.Config.OperationMode = mode.Value;
            repo.Save();
            return repo;
        }

        public Repository OpenRepositoryWithRetry(IAbsoluteDirectoryPath path, bool createWhenNotExisting = false,
            Action failAction = null) {
            if (createWhenNotExisting && !path.Exists)
                return new Repository(path, createWhenNotExisting);

            using (var autoResetEvent = new AutoResetEvent(false))
            using (var fileSystemWatcher =
                new FileSystemWatcher(path.ToString(), "*.lock") {
                    EnableRaisingEvents = true
                }) {
                var lockFile = path.GetChildFileWithName(Repository.LockFile);
                var fp = Path.GetFullPath(lockFile.ToString());
                fileSystemWatcher.Deleted +=
                    (o, e) => {
                        if (Path.GetFullPath(e.FullPath) == fp)
                            autoResetEvent.Set();
                    };

                while (true) {
                    using (var timer = UnlockTimer(lockFile, autoResetEvent)) {
                        try {
                            return new Repository(path, createWhenNotExisting);
                        } catch (RepositoryLockException) {
                            if (failAction != null)
                                failAction();
                            timer.Start();
                            autoResetEvent.WaitOne();
                            lock (timer)
                                timer.Stop();
                            autoResetEvent.Reset();
                        }
                    }
                }
            }
        }

        Timer UnlockTimer(IAbsoluteFilePath path, EventWaitHandle autoResetEvent) {
            var timer = new Timer();
            var pathS = path.ToString();
            timer.Elapsed += delegate {
                lock (timer) {
                    if (timer.Enabled) {
                        var unlocked = CheckFileClosed(pathS);
                        if (unlocked) {
                            // disable timer as soon as file is unlocked, to raise event only once!
                            timer.Enabled = false;
                            autoResetEvent.Set();
                        }

                        // set interval to 1 second after first elapse
                        timer.Interval = 1000;
                    }
                }
            };
            return timer;
        }

        bool CheckFileClosed(string path) {
            try {
                using (File.Open(path, FileMode.Open, FileAccess.Write, FileShare.None))
                    return true;
            } catch (Exception) {}
            return false;
        }

        public static bool IsEmpty(IAbsoluteDirectoryPath directory) {
            return !directory.Exists || !Directory.EnumerateFileSystemEntries(directory.ToString()).Any();
        }

        static void ConfirmEmpty(IAbsoluteDirectoryPath directory) {
            if (!IsEmpty(directory))
                throw new SynqPathException("Not empty!");
        }

        public void HandleRepositoryRequirements(IAbsoluteDirectoryPath repoPath) {
            ConfirmNotRootPath(repoPath);
            ConfirmNotRootedInAnotherRepository(repoPath);
        }

        static void ConfirmNotRootedInAnotherRepository(IAbsoluteDirectoryPath repoPath) {
            var parentRepo = Tools.FileUtil.FindParentWithName(repoPath.ToString(), Repository.DefaultRepoRootDirectory);
            if (parentRepo != null)
                throw new SynqPathException("May never be rooted in path with any other repository: " + repoPath);
        }

        static void ConfirmNotRootPath(IAbsoluteDirectoryPath path) {
            if (!path.HasParentDirectory)
                throw new SynqPathException("Path may not be the root of any drive: " + path);
        }

        public void HandlePackageRequirements(IAbsoluteDirectoryPath workPath, Repository repository) {
            HandlePathRequirements(workPath, repository);

            if (repository.Config.OperationMode == RepositoryOperationMode.SinglePackage)
                ConfirmSinglePackagePath(workPath, repository);
            else
                ConfirmMultiPackagePath(workPath, repository);
        }

        public void HandlePathRequirements(IAbsoluteDirectoryPath workPath, Repository repository) {
            ConfirmNotRootPath(workPath);
            ConfirmWorkingFolderNotRootedInRepository(workPath, repository);
        }

        static void ConfirmMultiPackagePath(IAbsoluteDirectoryPath workPath, Repository repository) {
            var repoParent = repository.RootPath.ParentDirectoryPath;
            if (Tools.FileUtil.IsPathRootedDirectlyIn(workPath, repoParent)) {
                // Good
                // .synq\, mod\*.*
                // MainLog.Logger.Debug("MP: Rooted Directly In!");
            } else {
                // Maybe
                if (Tools.FileUtil.IsPathRootedIn(repository.RootPath, workPath)) {
                    // BAD
                    // .synq\, *.* in any parent
                    throw new SynqPathException(
                        "The repository is in MultiPackage mode, the working directory may not be rooted in Repository parents");
                }

                //                if (Tools.FileUtil.IsPathRootedIn(workPath, repoParent)) {
                //                    // BAD
                //                    // .synq\, mod\x\y\*.*
                //                    throw new SynqPathException("May not open in subfolders");
                //                }

                // Good
                // completely different location
                //MainLog.Logger.Debug("MP: Completey different!");
            }
        }

        static void ConfirmSinglePackagePath(IAbsoluteDirectoryPath workPath, Repository repository) {
            if (Tools.FileUtil.IsPathRootedDirectlyIn(repository.RootPath, workPath)) {
                // Good
                // .synq\, *.*
                //MainLog.Logger.Debug("SP: Rooted Directly In!");
            } else {
                // Maybe
                if (Tools.FileUtil.IsPathRootedIn(repository.RootPath, workPath)) {
                    // BAD
                    // .synq\, *.* in any parent
                    throw new Exception(
                        "The repository is in SinglePackage mode, the working directory may not be rooted in Repository parents");
                }

                // GOOD
                // completely different location
                //MainLog.Logger.Debug("SP: Completey different!");
            }
        }

        static void ConfirmWorkingFolderNotRootedInRepository(IAbsoluteDirectoryPath workPath, Repository repository) {
            if (Tools.FileUtil.IsPathRootedIn(workPath, repository.RootPath, true)) {
                throw new Exception("Working folder may never be repository root or a subfolder: " + workPath + ", " +
                                    repository.RootPath);
            }
        }
    }
}