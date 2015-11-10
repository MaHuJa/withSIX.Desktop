// <copyright company="SIX Networks GmbH" file="ShortcutCreator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class ShortcutCreator
    {
        public static Task CreateDesktopPwsIcon(string name, string description, string arguments,
            IAbsoluteFilePath icon = null) {
            return Tools.FileUtil.CreateShortcutAsync(new ShortcutInfo(GetDesktop(), name,
                Common.Paths.EntryLocation) {
                    Arguments = arguments,
                    Icon = icon,
                    Description = description,
                    WorkingDirectory = Common.Paths.AppPath
                });
        }

        public static Task CreateDesktopPwsIconCustomRepo(string name, string description, Uri target,
            IAbsoluteFilePath icon = null) {
            return Tools.FileUtil.CreateShortcutAsync(new ShortcutInfo(GetDesktop(), name,
                Common.Paths.EntryLocation) {
                    Arguments = target.ToString(),
                    WorkingDirectory = Common.Paths.AppPath,
                    Description = description,
                    Icon = icon
                });
        }

        public static Task CreateDesktopGameIcon(string name, string description, string arguments, Game game,
            IAbsoluteFilePath icon = null) {
            return Tools.FileUtil.CreateShortcutAsync(new ShortcutInfo(GetDesktop(), name,
                game.InstalledState.LaunchExecutable) {
                    WorkingDirectory = game.InstalledState.Directory,
                    Arguments = arguments,
                    Description = description,
                    Icon = icon
                });
        }

        public static Task CreateDesktopGameBat(string name, string description, string arguments, Game game,
            IAbsoluteFilePath icon = null) {
            return
                Tools.FileUtil.CreateBatFile(GetDesktop(), name,
                    GenerateBatContent(game.InstalledState.LaunchExecutable, game.InstalledState.Directory, arguments));
            // TODO: DEscription etc? Maybe just create a nice shortcut to this thing incl icon?
        }

        static string GenerateBatContent(IAbsoluteFilePath target, IAbsoluteDirectoryPath workDir, string pars) {
            return string.Format(@"
@echo off
echo Starting: {0}
echo From: {1}
echo With params: {2}
cd /D ""{1}""
""{0}"" {2}", target, workDir, pars);
        }

        static IAbsoluteDirectoryPath GetDesktop() {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).ToAbsoluteDirectoryPath();
        }
    }
}