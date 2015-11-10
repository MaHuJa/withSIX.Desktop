// <copyright company="SIX Networks GmbH" file="MissionFolder.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy.Missions
{
    [DoNotObfuscate]
    public class MissionFolder : MissionBase
    {
        public MissionFolder(Guid id) : base(id) {}
        public string FolderName { get; set; }
        public override string ObjectTag
        {
            get { return FolderName; }
        }
        public override bool IsLocal
        {
            get { return true; }
        }

        public override Uri ProfileUrl() {
            throw new NotSupportedException("Local missions have no profile");
        }

        public override string CombinePath(string path, bool inclFolderName = true) {
            var fullPath = Path.Combine(path, PathType());
            return !inclFolderName ? fullPath : CombineFullPath(fullPath);
        }

        public override string CombineFullPath(string fullPath) {
            return Path.Combine(fullPath, FolderName);
        }
    }
}