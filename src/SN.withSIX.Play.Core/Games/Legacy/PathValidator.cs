// <copyright company="SIX Networks GmbH" file="PathValidator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public interface IModPathValidator
    {
        bool Validate(string path);
    }

    public abstract class PathValidator
    {
        static bool IsValidPath(string path) {
            return Tools.FileUtil.IsValidRootedPath(path);
        }

        protected static bool HasSubFolder(string path, string subFolder) {
            return Directory.Exists(Path.Combine(path, subFolder));
        }

        protected static bool ValidateBasics(string path) {
            return IsValidPath(path) && Directory.Exists(path);
        }
    }

    public class ArmaModPathValidator : PathValidator, IModPathValidator
    {
        static readonly string[] gameDataDirectories = {"addons", "dta", "common", "dll"};

        public bool Validate(string path) {
            return ValidateBasics(path) && ValidateSpecials(path);
        }

        public bool Validate(IAbsoluteDirectoryPath path) {
            return Validate(path.ToString());
        }

        static bool ValidateSpecials(string path) {
            return gameDataDirectories.Any(dir => HasSubFolder(path, dir));
        }
    }

    public class Homeworld2ModFileValidator : IModPathValidator
    {
        public bool Validate(string path) {
            return path.EndsWith(".big");
        }
    }
}