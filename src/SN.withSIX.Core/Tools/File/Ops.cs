// <copyright company="SIX Networks GmbH" file="Ops.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core
{
    public partial class Tools
    {
        public partial class FileTools
        {
            public class FileOps : IEnableLogging, IFileOps
            {
                public void Copy(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    if (FileUtil.ComparePathsEqualCase(source.ToString(), destination.ToString()))
                        throw new ArgumentException("Source and destination paths cannot be equal");

                    if (checkMd5 && SumsAreEqual(source, destination)) {
                        this.Logger()
                            .Info("Source and destination files equal. Source: {0}, Destination: {1}", source,
                                destination);
                        return;
                    }

                    File.Copy(source.ToString(), destination.ToString(), overwrite);
                }

                public void CopyWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    AddIORetryDialog(() => Copy(source, destination, overwrite, checkMd5), source.ToString(),
                        destination.ToString());
                }

                public Task CopyAsync(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    if (FileUtil.ComparePathsEqualCase(source.ToString(), destination.ToString()))
                        throw new ArgumentException("Source and destination paths cannot be equal");
                    if (!checkMd5)
                        return CopyAsyncInternal(source, destination, overwrite);
                    if (!destination.Exists || !source.Exists)
                        return CopyAsyncInternal(source, destination, overwrite);
                    if (HashEncryption.MD5FileHash(source) != HashEncryption.MD5FileHash(destination))
                        return CopyAsyncInternal(source, destination, overwrite);
                    this.Logger()
                        .Info("Source and destination files equal. Source: {0}, Destination: {1}", source,
                            destination);
                    return Task.FromResult(default(string));
                }

                public Task CopyAsyncWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false) {
                    return AddIORetryDialogAsync(() => CopyAsync(source, destination, overwrite, checkMd5),
                        source.ToString(), destination.ToString());
                }

                public void DeleteAfterRestart(string file) {
                    NativeMethods.MoveFileEx(file, null, MoveFileFlags.DelayUntilReboot);
                }

                public void MoveAfterRestart(string source, string destination) {
                    NativeMethods.MoveFileEx(source, destination, MoveFileFlags.DelayUntilReboot);
                }

                public void Move(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    if (FileUtil.ComparePathsEqualCase(source.ToString(), destination.ToString()))
                        throw new ArgumentException("Source and destination paths cannot be equal");

                    if (!source.Exists)
                        throw new FileNotFoundException("File doesnt exist: " + source);

                    if (checkMd5 && SumsAreEqual(source, destination)) {
                        this.Logger()
                            .Info("Source and destination files equal. Source: {0}, Destination: {1}", source,
                                destination);
                        if (!FileUtil.ComparePathsOsCaseSensitive(source, destination))
                            DeleteIfExists(source.ToString());
                        return;
                    }

                    source.RemoveReadonlyWhenExists();
                    if (FileUtil.ComparePathsOsCaseSensitive(source, destination))
                        CaseChangeMove(source, destination);
                    else
                        RealMove(source, destination, overwrite);
                }

                public void MoveWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    AddIORetryDialog(() => Move(source, destination, overwrite, checkMd5), source.ToString(),
                        destination.ToString());
                }

                public void CopyWithUpdaterFallbackAndRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false) {
                    AddIORetryDialog(() => {
                        try {
                            Copy(source, destination, overwrite, checkMd5);
                        } catch (UnauthorizedAccessException ex) {
                            this.Logger()
                                .FormattedWarnException(ex,
                                    "Exception during copy file, trying through updater if not elevated");
                            if (!Processes.Uac.CheckUac())
                                throw;
                            CopyWithUpdater(source, destination, overwrite);
                        }
                    }, source.ToString(), destination.ToString());
                }

                public void MoveWithUpdaterFallbackAndRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false) {
                    AddIORetryDialog(() => {
                        try {
                            Move(source, destination, overwrite, checkMd5);
                        } catch (UnauthorizedAccessException ex) {
                            this.Logger()
                                .FormattedWarnException(ex,
                                    "Exception during move file, trying through updater if not elevated");
                            if (!Processes.Uac.CheckUac())
                                throw;
                            MoveWithUpdater(source, destination, overwrite);
                        }
                    }, source.ToString(), destination.ToString());
                }

                public void DeleteDirectory(IAbsoluteDirectoryPath directory, bool overrideReadOnly = true) {
                    DeleteFileSystemInfo(directory.DirectoryInfo, overrideReadOnly);
                }

                public void DeleteFile(IAbsoluteFilePath file, bool overrideReadOnly = true) {
                    DeleteFileSystemInfo(new FileInfo(file.ToString()), overrideReadOnly);
                }

                public void DeleteIfExists(string path, bool overrideReadOnly = true) {
                    if (Directory.Exists(path))
                        DeleteDirectory(path.ToAbsoluteDirectoryPath(), overrideReadOnly);
                    else if (File.Exists(path))
                        DeleteFile(path.ToAbsoluteFilePath(), overrideReadOnly);
                }

                public void DeleteWithRetry(string path, bool overrideReadOnly = true) {
                    AddIORetryDialog(() => DeleteIfExists(path, overrideReadOnly), path);
                }

                public void DeleteFileSystemInfo(FileSystemInfo fsi, bool overrideReadonly = true) {
                    fsi.RemoveReadonlyWhenExists();
                    var di = fsi as DirectoryInfo;

                    if (di != null) {
                        foreach (var dirInfo in di.GetFileSystemInfos())
                            DeleteFileSystemInfo(dirInfo, overrideReadonly);
                    }

                    fsi.Delete();
                }

                public void DeleteWithUpdaterFallbackAndRetry(string source) {
                    AddIORetryDialog(() => {
                        try {
                            DeleteIfExists(source);
                        } catch (UnauthorizedAccessException ex) {
                            this.Logger()
                                .FormattedWarnException(ex,
                                    "Exception during delete, trying through updater if not elevated");
                            if (!Processes.Uac.CheckUac())
                                throw;
                            DeleteWithUpdater(source);
                        }
                    }, source);
                }

                public void CopyDirectory(IAbsoluteDirectoryPath sourceFolder, IAbsoluteDirectoryPath outputFolder,
                    bool overwrite = false) {
                    var di = sourceFolder.DirectoryInfo;

                    outputFolder.MakeSurePathExists();
                    CopyDirectoryFiles(outputFolder, di, overwrite);

                    foreach (var directory in di.EnumerateDirectories("*.*", SearchOption.AllDirectories)) {
                        CopyDirectoryFiles(
                            outputFolder.GetChildDirectoryWithName(directory.FullName.Replace(sourceFolder + "\\",
                                String.Empty)),
                            directory, overwrite);
                    }
                }

                public void CopyDirectoryWithRetry(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder, bool overwrite = false) {
                    var di = sourceFolder.DirectoryInfo;

                    outputFolder.MakeSurePathExists();
                    CopyDirectoryFilesWithRetry(outputFolder, di, overwrite);

                    foreach (var directory in di.EnumerateDirectories("*.*", SearchOption.AllDirectories)) {
                        CopyDirectoryFilesWithRetry(
                            outputFolder.GetChildDirectoryWithName(directory.FullName.Replace(sourceFolder + "\\",
                                String.Empty)),
                            directory, overwrite);
                    }
                }

                public void MoveDirectory(IAbsoluteDirectoryPath sourcePath, IAbsoluteDirectoryPath destinationPath) {
                    if (IsSameRoot(sourcePath, destinationPath)) {
                        var tmp = destinationPath + GenericTools.TmpExtension;
                        Directory.Move(sourcePath.ToString(), tmp);
                        Directory.Move(tmp, destinationPath.ToString());
                    } else {
                        CopyDirectoryWithRetry(sourcePath, destinationPath, true);
                        Directory.Delete(sourcePath.ToString(), true);
                    }
                }

                public void CreateText(IAbsoluteFilePath filePath, params string[] text) {
                    AddIORetryDialog(() => {
                        using (var fs = File.CreateText(filePath.ToString())) {
                            foreach (var t in text)
                                fs.Write(t);
                        }
                    }, filePath.ToString());
                }

                public DirectoryInfo CreateDirectory(string directoryPath) {
                    return Directory.CreateDirectory(directoryPath);
                }

                public DirectoryInfo CreateDirectoryWithRetry(string directoryPath) {
                    return AddIORetryDialog(() => CreateDirectory(directoryPath), directoryPath);
                }

                public void DowncasePath(string entry) {
                    var @base = Path.GetFileName(entry);
                    var dcBase = @base.ToLower();
                    if (@base == dcBase)
                        return;

                    var tmp = entry + GenericTools.TmpExtension;
                    var @new = Path.Combine(Path.GetDirectoryName(entry), dcBase);

                    if (File.Exists(tmp) || Directory.Exists(tmp))
                        DeleteWithRetry(tmp);

                    if (Directory.Exists(entry)) {
                        Directory.Move(entry, tmp);
                        if (File.Exists(@new) || Directory.Exists(@new))
                            DeleteWithRetry(@new);
                        Directory.Move(tmp, @new);
                    } else {
                        MoveWithRetry(entry.ToAbsoluteFilePath(), tmp.ToAbsoluteFilePath());
                        if (File.Exists(@new) || Directory.Exists(@new))
                            DeleteWithRetry(@new);
                        MoveWithRetry(tmp.ToAbsoluteFilePath(), @new.ToAbsoluteFilePath());
                    }
                }

                public void CopyDirectoryWithUpdaterFallbackAndRetry(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder, bool overwrite = false) {
                    AddIORetryDialog(() => {
                        try {
                            CopyDirectory(sourceFolder, outputFolder, overwrite);
                        } catch (UnauthorizedAccessException ex) {
                            this.Logger()
                                .FormattedWarnException(ex,
                                    "Exception during copy directory, trying through updater if not elevated");
                            if (!Processes.Uac.CheckUac())
                                throw;
                            CopyDirectoryWithUpdater(sourceFolder, outputFolder, overwrite);
                        } catch (IOException) {
                            if (!Processes.Uac.CheckUac())
                                throw;
                            CopyDirectoryWithUpdater(sourceFolder, outputFolder, overwrite);
                        }
                    }, sourceFolder.ToString(), outputFolder.ToString());
                }

                public string ReadTextFile(IAbsoluteFilePath path) {
                    using (var fs = new FileStream(path.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new StreamReader(fs))
                        return reader.ReadToEnd();
                }

                public string ReadTextFileWithRetry(IAbsoluteFilePath path) {
                    return AddIORetryDialog(() => ReadTextFile(path), path.ToString());
                }

                public byte[] ReadBinFileFromDisk(string path) {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        return fs.ToBytes();
                }

                public byte[] ReadBinFileFromDiskWithRetry(string path) {
                    return AddIORetryDialog(() => ReadBinFileFromDisk(path), path);
                }

                public void AddIORetryDialog(Action code, params string[] filePath) {
                    if (!Common.App.IsWpfApp) {
                        try {
                            code();
                        } catch (Exception e) {
                            LogIOException(e, filePath);
                            throw;
                        }
                        return;
                    }

                    while (true) {
                        try {
                            code();
                            return;
                        } catch (Exception e) {
                            if (!DialogIOException(e, filePath).Result)
                                throw;
                        }
                    }
                }

                public T AddIORetryDialog<T>(Func<T> code, params string[] filePath) {
                    if (!Common.App.IsWpfApp) {
                        try {
                            return code();
                        } catch (Exception e) {
                            LogIOException(e, filePath);
                            throw;
                        }
                    }

                    while (true) {
                        try {
                            return code();
                        } catch (Exception e) {
                            if (!DialogIOException(e, filePath).Result)
                                throw;
                        }
                    }
                }

                public async Task<T> AddIORetryDialogAsync<T>(Func<Task<T>> code, params string[] filePath) {
                    if (!Common.App.IsWpfApp) {
                        try {
                            return await code().ConfigureAwait(false);
                        } catch (Exception e) {
                            LogIOException(e, filePath);
                            throw;
                        }
                    }

                    while (true) {
                        ExceptionDispatchInfo ex;
                        try {
                            return await code().ConfigureAwait(false);
                        } catch (Exception e) {
                            ex = ExceptionDispatchInfo.Capture(e);
                        }
                        if (!await DialogIOException(ex.SourceException, filePath).ConfigureAwait(false))
                            ex.Throw();
                    }
                }

                public async Task AddIORetryDialogAsync(Func<Task> code, params string[] filePath) {
                    if (!Common.App.IsWpfApp) {
                        try {
                            await code().ConfigureAwait(false);
                        } catch (Exception e) {
                            LogIOException(e, filePath);
                            throw;
                        }
                        return;
                    }

                    while (true) {
                        ExceptionDispatchInfo ex;
                        try {
                            await code().ConfigureAwait(false);
                            return;
                        } catch (Exception e) {
                            ex = ExceptionDispatchInfo.Capture(e);
                        }
                        if (!await DialogIOException(ex.SourceException, filePath).ConfigureAwait(false))
                            ex.Throw();
                    }
                }

                public async Task<string> ReadTextFileAsync(IAbsoluteFilePath path) {
                    using (var fs = new FileStream(path.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new StreamReader(fs))
                        return await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                public Task CreateTextAsync(IAbsoluteFilePath filePath, params string[] text) {
                    return AddIORetryDialog(async () => {
                        using (var fs = File.CreateText(filePath.ToString())) {
                            foreach (var t in text)
                                await fs.WriteAsync(t).ConfigureAwait(false);
                        }
                    }, filePath.ToString());
                }

                void RealMove(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite) {
                    if (overwrite && destination.Exists &&
                        !FileUtil.ComparePathsOsCaseSensitive(source, destination))
                        DeleteIfExists(destination.ToString());
                    File.Move(source.ToString(), destination.ToString());
                }

                void CaseChangeMove(IAbsoluteFilePath source, IAbsoluteFilePath destination) {
                    var tmpFile = source + GenericTools.TmpExtension;
                    DeleteIfExists(tmpFile);
                    File.Move(source.ToString(), tmpFile);
                    File.Move(tmpFile, destination.ToString());
                }

                static bool SumsAreEqual(IAbsoluteFilePath source, IAbsoluteFilePath destination) {
                    if (!destination.Exists || !source.Exists)
                        return false;
                    return HashEncryption.MD5FileHash(source) == HashEncryption.MD5FileHash(destination);
                }

                public void MoveDirectoryWithUpdaterFallbackAndRetry(IAbsoluteDirectoryPath source,
                    IAbsoluteDirectoryPath destination) {
                    AddIORetryDialog(() => {
                        if (IsSameRoot(source, destination)) {
                            try {
                                var tmp = destination + GenericTools.TmpExtension;
                                Directory.Move(source.ToString(), tmp);
                                Directory.Move(tmp, destination.ToString());
                            } catch (UnauthorizedAccessException ex) {
                                this.Logger()
                                    .FormattedWarnException(ex,
                                        "Exception during move directory, trying through updater if not elevated");
                                if (!Processes.Uac.CheckUac())
                                    throw;
                                MoveDirectoryWithUpdater(source, destination);
                            }
                        } else {
                            var doneCopy = false;
                            try {
                                CopyDirectoryWithRetry(source, destination, true);
                                doneCopy = true;
                            } catch (UnauthorizedAccessException ex) {
                                this.Logger()
                                    .FormattedWarnException(ex,
                                        "Exception during move directory, trying through updater if not elevated");
                                if (!Processes.Uac.CheckUac())
                                    throw;
                                MoveDirectoryWithUpdater(source, destination);
                            }

                            if (doneCopy)
                                DeleteWithUpdaterFallbackAndRetry(source.ToString());
                        }
                    }, source.ToString(), destination.ToString());
                }

                static void MoveDirectoryWithUpdater(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder) {
                    Generic.RunUpdater(UpdaterCommands.MoveDirectory, sourceFolder.ToString(), outputFolder.ToString());
                }

                static async Task CopyAsyncInternal(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true) {
                    if (!overwrite && destination.Exists)
                        throw new IOException("destination file exists " + destination);
                    using (
                        var sourceStream = File.Open(source.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var destinationStream = File.Create(destination.ToString()))
                        await sourceStream.CopyToAsync(destinationStream).ConfigureAwait(false);
                }

                static void CopyWithUpdater(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true) {
                    Generic.RunUpdater(UpdaterCommands.Copy, source.ToString(), destination.ToString(),
                        overwrite ? "--overwrite" : null);
                }

                static void MoveWithUpdater(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true) {
                    Generic.RunUpdater(UpdaterCommands.Move, source.ToString(), destination.ToString(),
                        overwrite ? "--overwrite" : null);
                }

                static void DeleteWithUpdater(string source) {
                    Generic.RunUpdater(UpdaterCommands.Delete, source);
                }

                void CopyDirectoryFiles(IAbsoluteDirectoryPath outputFolder, DirectoryInfo di, bool overwrite = false) {
                    outputFolder.MakeSurePathExists();

                    foreach (var file in di.EnumerateFiles()) {
                        var dest = outputFolder.GetChildFileWithName(file.Name);
                        if (overwrite || !dest.Exists)
                            Copy(file.FullName.ToAbsoluteFilePath(), dest);
                    }
                }

                void CopyDirectoryFilesWithRetry(IAbsoluteDirectoryPath outputFolder, DirectoryInfo di,
                    bool overwrite = false) {
                    outputFolder.MakeSurePathExists();

                    foreach (var file in di.EnumerateFiles()) {
                        var dest = outputFolder.GetChildFileWithName(file.Name);
                        if (overwrite || !dest.Exists)
                            CopyWithRetry(file.FullName.ToAbsoluteFilePath(), dest);
                    }
                }

                static void CopyDirectoryWithUpdater(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder, bool overwrite = false) {
                    Generic.RunUpdater(UpdaterCommands.CopyDirectory, sourceFolder.ToString(), outputFolder.ToString(),
                        overwrite ? "--overwrite" : null);
                }

                void LogIOException(Exception e, params string[] filePath) {
                    if (!(e is IOException) && !(e is SecurityException) && !(e is UnauthorizedAccessException))
                        return;
                    this.Logger().FormattedDebugException(e);
                    Console.WriteLine(
                        "{0}: {1}\n\nIf the file is in use by another program, close it and retry. If the file is read-only, change it to be writable and retry.\n\nYou can also retry by running the application 'As Administrator'",
                        string.Join(", ", filePath), e.Message);
                }

                async Task<bool> DialogIOException(Exception e, params string[] filePath) {
                    if (!(e is IOException) && !(e is SecurityException) && !(e is UnauthorizedAccessException))
                        return false;

                    this.Logger().FormattedDebugException(e);
                    var result =
                        await UserError.Throw(new BasicUserError("Disk operation failed. Retry?",
                            string.Join(", ", filePath) + ": " + e.Message
                            + "\n\n" + GetIoRetryMessage(e), new[] {RecoveryCommand.Yes, RecoveryCommand.No}, null, e));
                    return result == RecoveryOptionResult.RetryOperation;
                }

                static string GetIoRetryMessage(Exception exception) {
                    if (exception is SecurityException || exception is UnauthorizedAccessException)
                        return "You can also retry by running the application 'As Administrator'";

                    return "If the file is in use by another program, close it and retry." +
                           " If the file is read-only, change it to be writable and retry."
                           + " If the harddisk is full, make some more free space";
                }

                string GetCurrentUserSDDL() {
                    return WindowsIdentity.GetCurrent().User.Value;
                }

                public void SetACL(IAbsolutePath location, string user = null,
                    FileSystemRights rights = FileSystemRights.FullControl) {
                    if (user == null)
                        user = GetCurrentUserSDDL();

                    if (Directory.Exists(location.ToString())) {
                        try {
                            SetDirectoryACL(location, user, rights);
                        } catch (InvalidOperationException) {
                            FixDirectoryACL(location);
                            SetDirectoryACL(location, user, rights);
                        }
                    } else if (File.Exists(location.ToString())) {
                        try {
                            SetFileACL(location, user, rights);
                        } catch (InvalidOperationException) {
                            FixFileACL(location);
                            SetFileACL(location, user, rights);
                        }
                    } else
                        throw new Exception("Path does not exist");
                }

                static void SetDirectoryACL(IAbsolutePath location, string user, FileSystemRights rights) {
                    var acl = Directory.GetAccessControl(location.ToString());
                    acl.SetAccessRule(GetAccessRule(user, rights));
                    Directory.SetAccessControl(location.ToString(), acl);
                }

                static void SetFileACL(IAbsolutePath location, string user, FileSystemRights rights) {
                    var acl = File.GetAccessControl(location.ToString());
                    acl.SetAccessRule(GetAccessRule(user, rights));
                    File.SetAccessControl(location.ToString(), acl);
                }

                // Fix for not caninonical issue...
                static void FixDirectoryACL(IAbsolutePath path) {
                    var directoryInfo = new DirectoryInfo(path.ToString());
                    var directorySecurity = directoryInfo.GetAccessControl(AccessControlSections.Access);
                    CanonicalizeDacl(directorySecurity);
                    directoryInfo.SetAccessControl(directorySecurity);
                }

                // Fix for not caninonical issue...
                static void FixFileACL(IAbsolutePath path) {
                    var fileInfo = new FileInfo(path.ToString());
                    var fileSecurity = fileInfo.GetAccessControl(AccessControlSections.Access);
                    CanonicalizeDacl(fileSecurity);
                    fileInfo.SetAccessControl(fileSecurity);
                }

                static void CanonicalizeDacl(NativeObjectSecurity objectSecurity) {
                    if (objectSecurity == null)
                        throw new ArgumentNullException("objectSecurity");

                    if (objectSecurity.AreAccessRulesCanonical)
                        return;

                    // A canonical ACL must have ACES sorted according to the following order:
                    //   1. Access-denied on the object
                    //   2. Access-denied on a child or property
                    //   3. Access-allowed on the object
                    //   4. Access-allowed on a child or property
                    //   5. All inherited ACEs 
                    var descriptor =
                        new RawSecurityDescriptor(
                            objectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));


                    var implicitDenyDacl = new List<CommonAce>();
                    var implicitDenyObjectDacl = new List<CommonAce>();
                    var inheritedDacl = new List<CommonAce>();
                    var implicitAllowDacl = new List<CommonAce>();
                    var implicitAllowObjectDacl = new List<CommonAce>();

                    foreach (CommonAce ace in descriptor.DiscretionaryAcl) {
                        if ((ace.AceFlags & AceFlags.Inherited) == AceFlags.Inherited)
                            inheritedDacl.Add(ace);
                        else {
                            switch (ace.AceType) {
                            case AceType.AccessAllowed:
                                implicitAllowDacl.Add(ace);
                                break;

                            case AceType.AccessDenied:
                                implicitDenyDacl.Add(ace);
                                break;

                            case AceType.AccessAllowedObject:
                                implicitAllowObjectDacl.Add(ace);
                                break;

                            case AceType.AccessDeniedObject:
                                implicitDenyObjectDacl.Add(ace);
                                break;
                            }
                        }
                    }

                    var aceIndex = 0;
                    var newDacl = new RawAcl(descriptor.DiscretionaryAcl.Revision, descriptor.DiscretionaryAcl.Count);
                    implicitDenyDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                    implicitDenyObjectDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                    implicitAllowDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                    implicitAllowObjectDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                    inheritedDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));

                    if (aceIndex != descriptor.DiscretionaryAcl.Count) {
                        throw new Exception(
                            "The DACL cannot be canonicalized since it would potentially result in a loss of information");
                    }

                    descriptor.DiscretionaryAcl = newDacl;
                    objectSecurity.SetSecurityDescriptorSddlForm(descriptor.GetSddlForm(AccessControlSections.Access),
                        AccessControlSections.Access);
                }

                public void CreateDirectoryAndSetACL(IAbsoluteDirectoryPath location, string user = null,
                    FileSystemRights rights = FileSystemRights.FullControl) {
                    if (user == null)
                        user = GetCurrentUserSDDL();
                    location.ToString().MakeSurePathExists();
                    SetACL(location, user, rights);
                }

                static FileSystemAccessRule GetAccessRule(string user, FileSystemRights rights) {
                    return new FileSystemAccessRule(new SecurityIdentifier(user), rights,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow);
                }

                public void SetACLWithFallbackAndRetry(IAbsolutePath location, string user = null,
                    FileSystemRights rights = FileSystemRights.FullControl) {
                    if (user == null)
                        user = GetCurrentUserSDDL();
                    AddIORetryDialog(() => {
                        try {
                            SetACL(location, user, rights);
                        } catch (UnauthorizedAccessException ex) {
                            this.Logger()
                                .FormattedWarnException(ex,
                                    "Exception during setACL, trying through updater if not elevated");
                            SetACLWithUpdater(location, user, rights);
                        }
                    });
                }

                static void SetACLWithUpdater(IAbsolutePath location, string user, FileSystemRights rights) {
                    Generic.RunUpdater(UpdaterCommands.SetAcl, location.ToString(), "--user=" + user,
                        "--rights=" + rights);
                }

                public void CreateDirectoryAndSetACLWithFallbackAndRetry(IAbsoluteDirectoryPath location,
                    string user = null,
                    FileSystemRights rights = FileSystemRights.FullControl) {
                    if (user == null)
                        user = GetCurrentUserSDDL();
                    AddIORetryDialog(() => {
                        try {
                            CreateDirectoryAndSetACL(location, user, rights);
                        } catch (UnauthorizedAccessException ex) {
                            this.Logger()
                                .FormattedWarnException(ex,
                                    "Exception during create directory and setACL, trying through updater if not elevated");
                            CreateDirectoryAndSetACLWithUpdater(location, user, rights);
                        }
                    });
                }

                static void CreateDirectoryAndSetACLWithUpdater(IAbsolutePath location, string user,
                    FileSystemRights rights) {
                    Generic.RunUpdater(UpdaterCommands.SetAcl, location.ToString(), "--user=" + user,
                        "--rights=" + rights,
                        "--createDirectory");
                }
            }

            [ContractClassFor(typeof (IFileOps))]
            public abstract class FileOpsContract : IFileOps
            {
                public void Copy(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentNullException>(destination != null);
                }

                public abstract void CopyWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false);

                public Task CopyAsync(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentNullException>(destination != null);

                    return default(Task);
                }

                public abstract Task CopyAsyncWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false);

                public void DeleteAfterRestart(string file) {
                    Contract.Requires<ArgumentNullException>(file != null);
                    Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(file));
                }

                public void MoveAfterRestart(string source, string destination) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(source));
                    Contract.Requires<ArgumentNullException>(destination != null);
                    Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(destination));
                }

                public void Move(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentNullException>(destination != null);
                }

                public abstract void MoveWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false);

                public void CopyWithUpdaterFallbackAndRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentNullException>(destination != null);
                }

                public void MoveWithUpdaterFallbackAndRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentNullException>(destination != null);
                }

                public abstract void DeleteDirectory(IAbsoluteDirectoryPath directory, bool overrideReadOnly = true);
                public abstract void DeleteFile(IAbsoluteFilePath file, bool overrideReadOnly = true);
                public abstract void DeleteIfExists(string path, bool overrideReadOnly = true);
                public abstract void DeleteWithRetry(string path, bool overrideReadOnly = true);
                public abstract void DeleteFileSystemInfo(FileSystemInfo fsi, bool overrideReadonly = true);

                public void DeleteWithUpdaterFallbackAndRetry(string source) {
                    Contract.Requires<ArgumentNullException>(source != null);
                    Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(source));
                }

                public void CopyDirectory(IAbsoluteDirectoryPath sourceFolder, IAbsoluteDirectoryPath outputFolder,
                    bool overwrite = false) {
                    Contract.Requires<ArgumentNullException>(sourceFolder != null);
                    Contract.Requires<ArgumentNullException>(outputFolder != null);
                }

                public void CopyDirectoryWithRetry(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder, bool overwrite = false) {
                    Contract.Requires<ArgumentNullException>(sourceFolder != null);
                    Contract.Requires<ArgumentNullException>(outputFolder != null);
                }

                public abstract void MoveDirectory(IAbsoluteDirectoryPath sourcePath,
                    IAbsoluteDirectoryPath destinationPath);

                public abstract void CreateText(IAbsoluteFilePath filePath, params string[] text);
                public abstract DirectoryInfo CreateDirectory(string directoryPath);
                public abstract DirectoryInfo CreateDirectoryWithRetry(string directoryPath);
                public abstract void DowncasePath(string entry);

                public void CopyDirectoryWithUpdaterFallbackAndRetry(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder, bool overwrite = false) {
                    Contract.Requires<ArgumentNullException>(sourceFolder != null);
                    Contract.Requires<ArgumentNullException>(outputFolder != null);
                }

                public abstract string ReadTextFile(IAbsoluteFilePath path);
                public abstract string ReadTextFileWithRetry(IAbsoluteFilePath path);
                public abstract byte[] ReadBinFileFromDisk(string path);
                public abstract byte[] ReadBinFileFromDiskWithRetry(string path);
                public abstract void AddIORetryDialog(Action code, params string[] filePath);
                public abstract T AddIORetryDialog<T>(Func<T> code, params string[] filePath);
                public abstract Task<T> AddIORetryDialogAsync<T>(Func<Task<T>> code, params string[] filePath);
                public abstract Task AddIORetryDialogAsync(Func<Task> code, params string[] filePath);

                public void Rename(string entry, string @new = null) {
                    Contract.Requires<ArgumentNullException>(entry != null);
                }
            }

            [ContractClass(typeof (FileOpsContract))]
            public interface IFileOps
            {
                void Copy(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false);

                void CopyWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false);

                Task CopyAsync(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false);

                Task CopyAsyncWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false);

                void DeleteAfterRestart(string file);
                void MoveAfterRestart(string source, string destination);

                void Move(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false);

                void MoveWithRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination, bool overwrite = true,
                    bool checkMd5 = false);

                void CopyWithUpdaterFallbackAndRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false);

                void MoveWithUpdaterFallbackAndRetry(IAbsoluteFilePath source, IAbsoluteFilePath destination,
                    bool overwrite = true, bool checkMd5 = false);

                void DeleteDirectory(IAbsoluteDirectoryPath directory, bool overrideReadOnly = true);
                void DeleteFile(IAbsoluteFilePath file, bool overrideReadOnly = true);
                void DeleteIfExists(string path, bool overrideReadOnly = true);
                void DeleteWithRetry(string path, bool overrideReadOnly = true);
                void DeleteFileSystemInfo(FileSystemInfo fsi, bool overrideReadonly = true);
                void DeleteWithUpdaterFallbackAndRetry(string source);

                void CopyDirectory(IAbsoluteDirectoryPath sourceFolder, IAbsoluteDirectoryPath outputFolder,
                    bool overwrite = false);

                void CopyDirectoryWithRetry(IAbsoluteDirectoryPath sourceFolder, IAbsoluteDirectoryPath outputFolder,
                    bool overwrite = false);

                void MoveDirectory(IAbsoluteDirectoryPath sourcePath, IAbsoluteDirectoryPath destinationPath);
                void CreateText(IAbsoluteFilePath filePath, params string[] text);
                DirectoryInfo CreateDirectory(string directoryPath);
                DirectoryInfo CreateDirectoryWithRetry(string directoryPath);
                void DowncasePath(string entry);

                void CopyDirectoryWithUpdaterFallbackAndRetry(IAbsoluteDirectoryPath sourceFolder,
                    IAbsoluteDirectoryPath outputFolder, bool overwrite = false);

                string ReadTextFile(IAbsoluteFilePath path);
                string ReadTextFileWithRetry(IAbsoluteFilePath path);
                byte[] ReadBinFileFromDisk(string path);
                byte[] ReadBinFileFromDiskWithRetry(string path);
                void AddIORetryDialog(Action code, params string[] filePath);
                T AddIORetryDialog<T>(Func<T> code, params string[] filePath);
                Task<T> AddIORetryDialogAsync<T>(Func<Task<T>> code, params string[] filePath);
                Task AddIORetryDialogAsync(Func<Task> code, params string[] filePath);
            }
        }
    }
}