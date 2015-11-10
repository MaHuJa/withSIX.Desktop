// <copyright company="SIX Networks GmbH" file="IAddonSigner.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;

namespace SN.withSIX.Sync.Core.ExternalTools
{
    [ContractClass(typeof (AddonSignerContract))]
    public interface IAddonSigner
    {
        void SignFile(IAbsoluteFilePath file, IAbsoluteFilePath privateFile, bool repackOnFailure = false);
        void SignFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile, bool repackOnFailure = false);
        void SignAllInFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile, bool repackOnFailure = false);
        void SignMany(AddonSigner.SignManyParams spec);
    }

    [ContractClassFor(typeof (IAddonSigner))]
    public abstract class AddonSignerContract : IAddonSigner
    {
        public void SignFile(IAbsoluteFilePath file, IAbsoluteFilePath privateFile, bool repackOnFailure = false) {
            Contract.Requires<ArgumentNullException>(file != null);
            Contract.Requires<ArgumentNullException>(privateFile != null);
            Contract.Requires<ArgumentOutOfRangeException>(!privateFile.FileName.Contains("@"));
        }

        public void SignFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(privateFile != null);
        }

        public void SignAllInFolder(IAbsoluteDirectoryPath folder, IAbsoluteFilePath privateFile,
            bool repackOnFailure = false) {
            Contract.Requires<ArgumentNullException>(folder != null);
            Contract.Requires<ArgumentNullException>(privateFile != null);
        }

        public void SignMany(AddonSigner.SignManyParams spec) {
            Contract.Requires<ArgumentNullException>(spec.Items != null);
        }
    }
}