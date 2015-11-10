// <copyright company="SIX Networks GmbH" file="AddonSigner.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Sync.Core.ExternalTools
{
    public class AddonSigner : IAddonSigner, IDomainService
    {
        readonly BiSigner _biSigner;

        public AddonSigner(BiSigner signer) {
            _biSigner = signer;
        }

        public void SignMany(SignManyParams spec) {
            ConfirmValidKeyOrPath(spec.PrivateFile, spec.KeyPath);

            foreach (var arg in spec.Items) {
                ProcessDirectoryOrFile(new ProcessDirectoryOrFileParams(spec.KeyPath, spec.Prefix,
                    spec.CopyKey, arg.ToString(), spec.PrivateFile) {
                        RepackIfFailed = spec.RepackIfFailed,
                        OnlyWhenMissing = spec.OnlyWhenMissing
                    });
            }
        }

        public void SignFile(IAbsoluteFilePath file, IAbsoluteFilePath privateFile, bool repackOnFailure = false) {
            _biSigner.SignFile(file, privateFile, repackOnFailure);
        }

        public void SignFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            _biSigner.SignFolder(folder, privateFile, repackOnFailure);
        }

        public void SignAllInFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            SignFolderRecursively(folder, privateFile, repackOnFailure);
        }

        public void SignFile(IAbsoluteFilePath file, BiKeyPair key) {
            _biSigner.SignFile(file, key.PrivateFile);
        }

        public void SignFolder(IAbsoluteDirectoryPath folder, BiKeyPair key) {
            _biSigner.SignFolder(folder, key.PrivateFile);
        }

        public void SignAllInFolder(IAbsoluteDirectoryPath folder, BiKeyPair key) {
            SignFolderRecursively(folder, key.PrivateFile);
        }

        static void ConfirmValidKeyOrPath(IAbsoluteFilePath privateFile, IAbsoluteDirectoryPath keyPath) {
            if (privateFile != null && !privateFile.Exists)
                throw new FileNotFoundException("Could not find the key {0}".FormatWith(privateFile));
            if (privateFile == null && (keyPath == null || !keyPath.Exists))
                throw new Exception("No key specified and no valid key path set");
        }

        void SignFolderRecursively(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            foreach (var f in Directory.EnumerateFiles(folder.ToString(), "*.pbo", SearchOption.AllDirectories))
                _biSigner.SignFile(f.ToAbsoluteFilePath(), privateFile, repackOnFailure);
        }

        void ProcessDirectoryOrFile(ProcessDirectoryOrFileParams spec) {
            if (Directory.Exists(spec.Arg)) {
                ProcessDirectory(spec, spec.Arg.ToAbsoluteDirectoryPath());
                return;
            }
            if (!File.Exists(spec.Arg))
                throw new Exception("Cannot find {0}".FormatWith(spec.Arg));
            ProcessFile(spec, spec.Arg.ToAbsoluteFilePath());
        }

        void ProcessDirectory(ProcessDirectoryOrFileParams spec, IAbsoluteDirectoryPath directory) {
            if (spec.OnlyWhenMissing) {
                ProcessDirectoryOnlyMissing(spec, directory);
                return;
            }
            var biKeyPair = GetKey(spec, directory);
            _biSigner.SignFolder(directory, biKeyPair.PrivateFile, spec.RepackIfFailed);
        }

        void ProcessDirectoryOnlyMissing(ProcessDirectoryOrFileParams spec, IAbsoluteDirectoryPath directory) {
            var biKeys = directory.DirectoryInfo.EnumerateFiles("*.bikey", SearchOption.AllDirectories)
                .Select(x => new BiKeyFile(x)).ToArray();
            var pboFiles =
                directory.DirectoryInfo.EnumerateFiles("*.pbo", SearchOption.AllDirectories);
            var signFiles =
                directory.DirectoryInfo.EnumerateFiles("*.bisign", SearchOption.AllDirectories)
                    .Select(x => new BiSignFile(x));
            var validSignfiles = signFiles.Where(s => biKeys.Any(k => MatchesKey(k, s))).ToArray();
            var todoFiles =
                pboFiles.Where(f => !validSignfiles.Any(s => MatchesBiSign(s, f)))
                    .ToArray();
            if (todoFiles.Length <= 0)
                return;

            var biKeyP = GetKey(spec, directory);
            foreach (var f in todoFiles)
                _biSigner.SignFile(f.FullName.ToAbsoluteFilePath(), biKeyP.PrivateFile, spec.RepackIfFailed);
        }

        static bool MatchesKey(BiKeyFile biKeyFile, BiSignFile biSignFile) {
            return biSignFile.KeyName.Equals(biKeyFile.KeyName, StringComparison.InvariantCultureIgnoreCase);
        }

        static bool MatchesBiSign(BiSignFile s, FileInfo f) {
            var projectedBisign = (f.FullName + "." + s.KeyName + ".bisign");
            return projectedBisign.Equals(s.FilePath.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        static bool MatchesBiSign(string s, string f) {
            return s.StartsWith(f, StringComparison.InvariantCultureIgnoreCase) &&
                   s.EndsWith(".bisign", StringComparison.InvariantCultureIgnoreCase);
        }

        void ProcessFile(ProcessDirectoryOrFileParams spec, IAbsoluteFilePath filePath) {
            if (spec.OnlyWhenMissing &&
                filePath.FileInfo.Directory.EnumerateFiles("*.bisign")
                    .Any(s => MatchesBiSign(s.FullName, filePath.ToString())))
                return;

            var biKeyPair = GetKey(spec, filePath.ParentDirectoryPath);
            _biSigner.SignFile(filePath, biKeyPair.PrivateFile, spec.RepackIfFailed);
        }

        BiKeyPair GetKey(ProcessDirectoryOrFileParams spec, IAbsoluteDirectoryPath directory) {
            return _biSigner.HandleKey(new BiSigner.HandleKeyParams(spec.KeyPath, spec.Prefix, spec.CopyKey, directory,
                spec.Key));
        }

        class BiKeyFile
        {
            public BiKeyFile(IAbsoluteFilePath filePath) {
                FilePath = filePath;
                KeyName = filePath.FileNameWithoutExtension;
            }

            public BiKeyFile(string filePath) : this(filePath.ToAbsoluteFilePath()) {}
            public BiKeyFile(FileInfo filePath) : this(filePath.FullName.ToAbsoluteFilePath()) {}
            public string KeyName { get; }
            public IAbsoluteFilePath FilePath { get; private set; }
        }

        class BiSignFile
        {
            public BiSignFile(IAbsoluteFilePath filePath) {
                FilePath = filePath;
                var indexOf = filePath.FileNameWithoutExtension.IndexOf(".pbo");
                Directory = filePath.ParentDirectoryPath;
                KeyName = filePath.FileNameWithoutExtension.Substring(indexOf + 5);
                PboName = filePath.FileNameWithoutExtension.Substring(0, indexOf + 4);
            }

            public BiSignFile(string filePath) : this(filePath.ToAbsoluteFilePath()) {}
            public BiSignFile(FileInfo filePath) : this(filePath.FullName.ToAbsoluteFilePath()) {}
            public string PboName { get; private set; }
            public IAbsoluteDirectoryPath Directory { get; private set; }
            public string KeyName { get; }
            public IAbsoluteFilePath FilePath { get; }
        }

        class ProcessDirectoryOrFileParams
        {
            public ProcessDirectoryOrFileParams(IAbsoluteDirectoryPath keyPath, string prefix, bool copyKey,
                string arg, IAbsoluteFilePath key) {
                KeyPath = keyPath;
                Prefix = prefix;
                CopyKey = copyKey;
                Arg = arg;
                Key = key;
            }

            public IAbsoluteDirectoryPath KeyPath { get; }
            public string Prefix { get; }
            public bool CopyKey { get; }
            public string Arg { get; }
            public IAbsoluteFilePath Key { get; }
            public bool RepackIfFailed { get; set; }
            public bool OnlyWhenMissing { get; set; }
        }

        public class SignManyParams
        {
            public SignManyParams(IPath[] items, IAbsoluteFilePath privateFile, IAbsoluteDirectoryPath keyPath,
                string prefix, bool copyKey) {
                Items = items;
                PrivateFile = privateFile;
                KeyPath = keyPath;
                Prefix = prefix;
                CopyKey = copyKey;
            }

            public bool RepackIfFailed { get; set; }
            public IPath[] Items { get; }
            public IAbsoluteFilePath PrivateFile { get; }
            public IAbsoluteDirectoryPath KeyPath { get; }
            public string Prefix { get; }
            public bool CopyKey { get; }
            public bool OnlyWhenMissing { get; set; }
        }
    }
}