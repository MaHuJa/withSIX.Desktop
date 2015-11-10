// <copyright company="SIX Networks GmbH" file="SignCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.ExternalTools;
using SN.withSIX.Sync.Core.Packages;

namespace SN.withSIX.Sync.Presentation.Console.Commands
{
    [DoNotObfuscateType]
    public class SignCommand : BaseCommand
    {
        readonly IAddonSigner _signer;
        public bool CopyKey;
        public string Key;
        public string KeyPath;
        public bool Missing;
        public string Prefix;
        public bool Repack;

        public SignCommand(IAddonSigner signer) {
            _signer = signer;
            IsCommand("sign", "Signature management");
            HasOption("p|prefix=", "Prefix for key creation", s => Prefix = s);
            HasOption("k|key=", "Specify biprivatekey to use", s => Key = s);
            HasOption("keypath=", "Specify the path for created key (If no key specified)", s => KeyPath = s);
            HasOption("c|copykey", "Copy the public key to keys subfolder of folders", s => CopyKey = s != null);
            HasOption("r|repack", "Repack on failure", r => Repack = r != null);
            HasOption("m|missing", "Missing only", m => Missing = m != null);
            AllowsAnyAdditionalArguments("<folder or file> (<folder or file>...)");
        }

        public override int Run(params string[] remainingArguments) {
            if (!remainingArguments.Any())
                throw new ConsoleHelpAsException("Please specify at least one folder or file to sign");

            if (Key != null && KeyPath != null)
                throw new Exception("Cannot set both KeyPath and Key");

            /*
             * Key specified: Use key, throw error when not found
             * Key unspecified: Use existing key, or create new. Needs default key location setting
             * CopyKey: Copy public key to keys subfolder of folders
             * 
             * TODO: Should we support e.g H:\rsync, where there are multiple modfolders to sign, each with own key etc?
             * TODO: Support for Date and Version based keys
             * 
             * Folder: Look for Addons, Dta folders, and *.pbo inside
             * File: Sign file
             */

            var keyFilePath = Key == null ? null : Key.ToAbsoluteFilePath();
            var keyPath = KeyPath == null ? null : KeyPath.ToAbsoluteDirectoryPath();

            var paths =
                remainingArguments.Select(
                    x => Directory.Exists(x) ? (IPath) x.ToAbsoluteDirectoryPath() : x.ToFilePath());

            // TODO: SignFolder does not support sub-modfolders, like: beta_oa\expansion\addons etc, just beta_oa\addons etc

            var exclusions = Exclusions(paths).ToArray();
            if (exclusions.Any()) {
                System.Console.WriteLine("Excluding the following folder(s): {0}",
                    string.Join(", ", exclusions.Select(x => x.ToString())));
            }

            _signer.SignMany(new AddonSigner.SignManyParams(paths.Except(exclusions).ToArray(), keyFilePath, keyPath,
                Prefix, CopyKey) {
                    RepackIfFailed = Repack,
                    OnlyWhenMissing = Missing
                });

            return 0;
        }

        static IEnumerable<IPath> Exclusions(IEnumerable<IPath> remainingArguments) {
            return
                remainingArguments.OfType<IAbsoluteDirectoryPath>()
                    .Where(x => !Package.ReadSynqSpec(x).Processing.Sign.GetValueOrDefault(true));
        }
    }
}