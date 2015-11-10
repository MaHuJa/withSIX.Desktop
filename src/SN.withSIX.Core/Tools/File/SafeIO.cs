// <copyright company="SIX Networks GmbH" file="SafeIO.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NDepend.Path;

namespace SN.withSIX.Core
{
    public partial class Tools
    {
        public partial class FileTools
        {
            public class SafeIO
            {
                public static void SafeSave(Action<IAbsoluteFilePath> saveCode, IAbsoluteFilePath filePath) {
                    var newFileName = (filePath + GenericTools.TmpExtension).ToAbsoluteFilePath();

                    saveCode(newFileName);
                    FileUtil.Ops.Copy(newFileName, filePath);
                    FileUtil.Ops.DeleteFile(newFileName);
                }
            }
        }
    }
}