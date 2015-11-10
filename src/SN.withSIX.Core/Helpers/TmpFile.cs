// <copyright company="SIX Networks GmbH" file="TmpFile.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using NDepend.Path;

namespace SN.withSIX.Core.Helpers
{
    public class TmpFile : IDisposable
    {
        public TmpFile(string file) : this(file.ToAbsoluteFilePath()) {}

        public TmpFile(IAbsoluteFilePath file) {
            FilePath = file;
        }

        public IAbsoluteFilePath FilePath { get; }

        public void Dispose() {
            if (FilePath.Exists)
                FilePath.FileInfo.Delete();
        }
    }

    public class TmpFileCreated : TmpFile
    {
        public TmpFileCreated()
            : base(Path.GetTempFileName()) {}
    }

    public class TmpDirectory : IDisposable
    {
        readonly IAbsoluteDirectoryPath _path;
        public TmpDirectory(string path) : this(path.ToAbsoluteDirectoryPath()) {}

        public TmpDirectory(IAbsoluteDirectoryPath path) {
            path.MakeSurePathExists();
            _path = path;
        }

        public void Dispose() {
            if (_path.Exists)
                _path.DirectoryInfo.Delete(true);
        }
    }
}