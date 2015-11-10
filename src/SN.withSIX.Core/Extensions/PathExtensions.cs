// <copyright company="SIX Networks GmbH" file="PathExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NDepend.Path;

namespace SN.withSIX.Core.Extensions
{
    public static class PathExtensions
    {
        static readonly Regex RxDrive = new Regex(@"^([a-z]):", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string EscapePath(this string arg) {
            return String.Format("\"{0}\"", arg);
        }

        public static IAbsoluteDirectoryPath ToAbsoluteDirectoryPathNullSafe(this string path) {
            return string.IsNullOrEmpty(path) ? null : path.ToAbsoluteDirectoryPath();
        }

        public static IAbsoluteFilePath ToAbsoluteFilePathNullSafe(this string path) {
            return string.IsNullOrEmpty(path) ? null : path.ToAbsoluteFilePath();
        }

        public static string EscapePath(this IAbsolutePath arg) {
            return String.Format("\"{0}\"", arg);
        }

        public static IEnumerable<DirectoryInfo> FilterDotted(this IEnumerable<DirectoryInfo> infos) {
            return infos.Where(x => !StartsWithDot(x));
        }

        public static IEnumerable<FileInfo> FilterDotted(this IEnumerable<FileInfo> infos) {
            return infos.Where(x => !StartsWithDot(x));
        }

        static bool StartsWithDot(FileSystemInfo x) {
            return x.Name.StartsWith(".");
        }

        public static IEnumerable<DirectoryInfo> RecurseFilterDottedDirectories(this DirectoryInfo di) {
            var filterDottedDirectories = di.FilterDottedDirectories();
            return filterDottedDirectories;
            //.Concat(filterDottedDirectories.SelectMany(RecurseFilterDottedDirectories));
        }

        public static IEnumerable<DirectoryInfo> FilterDottedDirectories(this DirectoryInfo di) {
            return di.EnumerateDirectories().FilterDotted();
        }

        public static IEnumerable<FileInfo> FilterDottedFiles(this DirectoryInfo di) {
            return di.EnumerateFiles().FilterDotted();
        }

        public static string CleanPath(this string path) {
            return path.Replace("/", "\\").TrimEnd('\\');
        }

        public static string CygwinPath(this string arg) {
            arg = PosixSlash(arg);
            var match = RxDrive.Match(arg);
            return match.Success ? arg.Replace(match.Value, "/cygdrive/" + match.Groups[1].Value.ToLower()) : arg;
        }

        public static string MingwPath(this string arg) {
            arg = PosixSlash(arg);
            var match = RxDrive.Match(arg);
            return match.Success ? arg.Replace(match.Value, "/" + match.Groups[1].Value.ToLower()) : arg;
        }

        public static string PosixSlash(this string arg) {
            return arg.Replace('\\', '/');
        }
    }
}