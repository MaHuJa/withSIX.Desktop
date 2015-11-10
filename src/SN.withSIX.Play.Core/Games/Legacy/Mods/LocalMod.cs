// <copyright company="SIX Networks GmbH" file="LocalMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using NDepend.Path;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Validators;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DoNotObfuscate]
    public class LocalMod : Mod
    {
        public LocalMod(Guid id) : base(id) {
            Categories = new[] {Common.DefaultCategory};
        }

        public bool IsLocal
        {
            get { return true; }
        }

        public override string GetSerializationString() {
            var path = CustomPath;
            return path != null ? path.GetChildDirectoryWithName(Name).ToString() : Name;
        }

        public static IMod FromStringIfValid(string modPath, ISupportModding game) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(modPath));
            Contract.Requires<ArgumentNullException>(game != null);

            if (modPath.IsValidAbsoluteFilePath()) {
                var filePath = modPath.ToAbsoluteFilePath();
                return CreateLocalMod(filePath.FileName, filePath.ParentDirectoryPath, game);
            }

            if (modPath.IsValidAbsoluteDirectoryPath()) {
                var dirPath = modPath.ToAbsoluteDirectoryPath();
                return CreateLocalMod(dirPath.DirectoryName, dirPath.ParentDirectoryPath, game);
            }

            if (!FileNameValidator.IsValidName(modPath))
                return null;

            // TODO: Horrible non-determinism; is it a file, is it a folder, is it located in the game or mod dir?
            var modPaths = game.ModPaths;
            if (modPaths.IsValid) {
                try {
                    if (modPaths.Path.GetChildDirectoryWithName(modPath).Exists)
                        return CreateLocalMod(modPath, modPaths.Path, game);
                } catch (Exception) {}
                try {
                    if (modPaths.Path.GetChildFileWithName(modPath).Exists)
                        return CreateLocalMod(modPath, modPaths.Path, game);
                } catch (Exception) {}
            }

            var installedState = game.InstalledState;
            if (installedState.IsInstalled) {
                try {
                    if (installedState.Directory.GetChildDirectoryWithName(modPath).Exists)
                        return CreateLocalMod(modPath, installedState.Directory, game);
                } catch (Exception) {}
                try {
                    if (installedState.Directory.GetChildFileWithName(modPath).Exists)
                        return CreateLocalMod(modPath, installedState.Directory, game);
                } catch (Exception) {}
            }

            return CreateLocalMod(modPath, null, game);
        }

        public static IMod FromLocalModInfo(LocalModInfo info, ISupportModding game) {
            Contract.Requires<ArgumentNullException>(info != null);
            Contract.Requires<ArgumentNullException>(game != null);
            return CreateLocalMod(info.Name, info.Path, game);
        }

        static LocalMod CreateLocalMod(string name, IAbsoluteDirectoryPath localModPath, ISupportModding game) {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(game != null);

            var localMod = new LocalMod(System.Guid.Empty) {
                Name = name,
                CustomPath = localModPath
            };
            localMod.Controller.UpdateState(game);
            return localMod;
        }

        public override Uri ProfileUrl() {
            throw new NotSupportedException("Local mods have no profile");
        }

        protected override string GetSlugType() {
            return typeof (Mod).Name.ToUnderscore() + "s";
        }
    }
}