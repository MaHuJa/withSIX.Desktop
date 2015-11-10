// <copyright company="SIX Networks GmbH" file="BiKeyPair.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using NDepend.Path;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.ExternalTools
{
    public class BiKeyPair : PropertyChangedBase
    {
        bool _isSelected;

        public BiKeyPair(IAbsoluteFilePath path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentOutOfRangeException>(!path.FileName.Contains("@"));

            var p = path.ToString().Replace(".biprivatekey", String.Empty).Replace(".bikey", String.Empty);
            Location = Path.GetDirectoryName(p);
            Name = Path.GetFileName(p);
            CreatedAt = File.GetCreationTime(p);
            PrivateFile = (p + ".biprivatekey").ToAbsoluteFilePath();
            PublicFile = (p + ".bikey").ToAbsoluteFilePath();
        }

        public IAbsoluteFilePath PrivateFile { get; }
        public IAbsoluteFilePath PublicFile { get; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; protected set; }
        public string Location { get; protected set; }

        public static BiKeyPair CreateSignKey(IAbsoluteFilePath path, PboTools pboTools) {
            CreateKey(path, pboTools);
            return new BiKeyPair(path);
        }

        public static void CreateKey(IAbsoluteFilePath path, PboTools pboTools) {
            Contract.Requires<ArgumentOutOfRangeException>(!path.FileName.Contains("@"));
            Contract.Requires<ArgumentOutOfRangeException>(!path.FileName.Contains(".biprivatekey"));
            Contract.Requires<ArgumentOutOfRangeException>(!path.FileName.Contains(".bikey"));
            pboTools.CreateKey(path);
        }
    }
}