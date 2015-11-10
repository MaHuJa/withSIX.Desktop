// <copyright company="SIX Networks GmbH" file="EnumerateSignatures.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public class EnumerateSignatures
    {
        public IEnumerable<string> Enumerate(IAbsoluteDirectoryPath path) {
            Contract.Requires<ArgumentNullException>(path != null);
            return Tools.FileUtil.GetFiles(path, "*.bisign")
                .Select(GetSignatureFromFileName).Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.ToLower())
                .Distinct();
        }

        static string GetSignatureFromFileName(FileInfo x) {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(x.Name);
            var index = fileNameWithoutExtension.LastIndexOf(".pbo.");
            return index > -1 ? fileNameWithoutExtension.Substring(index + 5) : null;
        }
    }
}