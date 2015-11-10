// <copyright company="SIX Networks GmbH" file="DirectoryExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using NDepend.Path;

namespace SN.withSIX.Core.Extensions
{
    public static class DirectoryExtensions
    {
        public static string Join(this IRelativeDirectoryPath relative) {
            var parts = new List<string>();
            parts.Insert(0, relative.DirectoryName);

            while (relative.HasParentDirectory) {
                relative = relative.ParentDirectoryPath;
                parts.Insert(0, relative.DirectoryName);
            }
            return Path.Combine(parts.ToArray());
        }

        public static bool IsRootedIn(this IAbsolutePath path, IAbsoluteDirectoryPath possibleRoot) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(possibleRoot != null);
            while (path.HasParentDirectory) {
                if (path.ParentDirectoryPath.Equals(possibleRoot))
                    return true;
                path = path.ParentDirectoryPath;
            }
            return false;
        }

        public static string GetRelativeDirectory(this IAbsoluteDirectoryPath path,
            IAbsoluteDirectoryPath possibleRoot) {
            if (path.Equals(possibleRoot) || !path.CanGetRelativePathFrom(possibleRoot) ||
                !path.IsRootedIn(possibleRoot))
                return path.ToString();

            return path.GetRelativePathFrom(possibleRoot).Join();
        }
    }
}