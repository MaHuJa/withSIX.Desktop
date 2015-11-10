// <copyright company="SIX Networks GmbH" file="BiSigner.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.ExternalTools
{
    public class BiSigner : IDomainService
    {
        static readonly string[] addonFolders = {"addons", "dta", "optional"};
        readonly PboTools _pboTools;

        public BiSigner(PboTools pboTools) {
            _pboTools = pboTools;
        }

        public void SignFile(IAbsoluteFilePath file, IAbsoluteFilePath privateFile, bool repackOnFailure = false) {
            try {
                _pboTools.SignFile(file, privateFile);
            } catch (ProcessException) {
                if (!repackOnFailure)
                    throw;
                _pboTools.RepackPbo(file);
                _pboTools.SignFile(file, privateFile);
            }
        }

        public void SignFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            SignFolderNotRecursively(folder, privateFile, repackOnFailure);

            foreach (var d in addonFolders
                .Select(folder.GetChildDirectoryWithName)
                .Where(x => x.Exists))
                SignFolderNotRecursively(d, privateFile, repackOnFailure);
        }

        public BiKeyPair HandleKey(HandleKeyParams spec) {
            if (spec.Key == null) {
                spec.Key = GetDefaultPrivateKeyPath(spec.KeyPath, spec.Prefix,
                    spec.Directory);
            }
            var signKey = new BiKeyPair(spec.Key);
            if (!signKey.PrivateFile.Exists)
                BiKeyPair.CreateKey(Path.Combine(signKey.Location, signKey.Name).ToAbsoluteFilePath(), _pboTools);
            if (spec.CopyKey)
                CopyKeyToKeysSubfolder(signKey, spec.Directory);
            return signKey;
        }

        static IAbsoluteFilePath GetDefaultPrivateKeyPath(IAbsoluteDirectoryPath keyPath, string prefix,
            IAbsoluteDirectoryPath arg) {
            return
                keyPath.GetChildFileWithName((prefix ?? String.Empty) + arg.DirectoryName.Replace("@", String.Empty) +
                                             ".biprivatekey");
        }

        static void CopyKeyToKeysSubfolder(BiKeyPair key, IAbsoluteDirectoryPath destination) {
            var keysFolder = destination.GetChildDirectoryWithName("keys");
            keysFolder.MakeSurePathExists();
            Tools.FileUtil.Ops.CopyWithRetry(key.PublicFile,
                keysFolder.GetChildFileWithName(key.PublicFile.FileName));
        }

        void SignFolderNotRecursively(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            foreach (var f in Directory.EnumerateFiles(folder.ToString(), "*.pbo"))
                SignFile(f.ToAbsoluteFilePath(), privateFile, repackOnFailure);
        }

        public class HandleKeyParams
        {
            public HandleKeyParams(IAbsoluteDirectoryPath keyPath, string prefix, bool copyKey,
                IAbsoluteDirectoryPath directory, IAbsoluteFilePath key) {
                KeyPath = keyPath;
                Prefix = prefix;
                CopyKey = copyKey;
                Directory = directory;
                Key = key;
            }

            public IAbsoluteDirectoryPath KeyPath { get; }
            public string Prefix { get; }
            public bool CopyKey { get; }
            public IAbsoluteDirectoryPath Directory { get; }
            public IAbsoluteFilePath Key { get; set; }
        }
    }
}