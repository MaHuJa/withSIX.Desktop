// <copyright company="SIX Networks GmbH" file="Restarter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core.Applications.Services
{
    public interface IRestarter
    {
        Task<bool> CheckUac(IAbsoluteDirectoryPath mp);
        Task<bool> TryWithUacFallback(Task task, string info);
        void RestartWithoutElevation(params string[] args);
        void RestartInclEnvironmentCommandLine();
        void RestartWithUacInclEnvironmentCommandLine();
    }

    public class Restarter : IApplicationService, IEnableLogging, IRestarter
    {
        readonly IDialogManager _dialogManager;
        readonly IShutdownHandler _shutdownHandler;

        public Restarter(IShutdownHandler shutdownHandler, IDialogManager dialogManager) {
            _shutdownHandler = shutdownHandler;
            _dialogManager = dialogManager;
        }

        public async Task<bool> CheckUac(IAbsoluteDirectoryPath mp) {
            return Tools.Processes.Uac.CheckUac() &&
                   await TryCheckUac(mp, mp.GetChildFileWithName("_play_withSIX_testFile.txt")).ConfigureAwait(false);
        }

        public async Task<bool> TryWithUacFallback(Task task, string info) {
            if (!Tools.Processes.Uac.CheckUac()) {
                await task.ConfigureAwait(false);
                return false;
            }
            Exception e;
            try {
                await task.ConfigureAwait(false);
                return false;
            } catch (UnauthorizedAccessException ex) {
                e = ex;
            }
            var report = await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(
                String.Format(
                    "The application failed to write to the path, probably indicating permission issues\nWould you like to restart the application Elevated?\n\n {0}\n{1}",
                    info, e.Message),
                "Restart the application elevated?", SixMessageBoxButton.YesNo)) == SixMessageBoxResult.Yes;

            UsageCounter.ReportUsage("Dialog - Restart the application elevated: {0}".FormatWith(report));

            if (!report)
                throw e;
            RestartWithUacInclEnvironmentCommandLine();
            return true;
        }

        public void RestartWithoutElevation(params string[] args) {
            Restart(false, true, args);
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void RestartInclEnvironmentCommandLine() {
            Restart(false, true, Tools.Generic.GetStartupParameters().ToArray());
        }

        [SmartAssembly.Attributes.ReportUsage]
        public void RestartWithUacInclEnvironmentCommandLine() {
            Restart(true, true, Tools.Generic.GetStartupParameters().ToArray());
        }

        async Task<bool> TryCheckUac(IAbsoluteDirectoryPath mp, IAbsoluteFilePath path) {
            Exception ex;
            try {
                mp.MakeSurePathExists();
                if (path.Exists)
                    File.Delete(path.ToString());
                using (File.CreateText(path.ToString())) {}
                File.Delete(path.ToString());
                return false;
            } catch (UnauthorizedAccessException e) {
                ex = e;
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
                return false;
            }

            var report = await UserError.Throw(new BasicUserError("Restart the application elevated?",
                String.Format(
                    "The application failed to write to the path, probably indicating permission issues\nWould you like to restart the application Elevated?\n\n {0}",
                    mp), RecoveryCommands.YesNoCommands, null, ex)) == RecoveryOptionResult.RetryOperation;

            if (!report)
                return false;
            RestartWithUacInclEnvironmentCommandLine();
            return true;
        }

        void Restart(bool elevated = false, bool exit = false, params string[] args) {
            var ps = GetSquirrelRestart(args);
            if (elevated)
                ps.Verb = "runas";

            Common.OnExit = () => { using (Process.Start(ps)) {} };

            if (exit)
                _shutdownHandler.Shutdown();
        }

        static ProcessStartInfo GetSelfUpdaterRestart(IEnumerable<string> args) {
            return new ProcessStartInfo {
                FileName = Common.Paths.SelfUpdaterExePath.ToString(),
                Arguments =
                    new[] {SelfUpdaterCommands.RestartCommand, Common.Paths.EntryLocation.ToString()}.Concat(args)
                        .CombineParameters()
            };
        }

        static ProcessStartInfo GetSquirrelRestart(IEnumerable<string> args) {
            return new ProcessStartInfo {
                FileName = Common.Paths.AppPath.ParentDirectoryPath.GetChildFileWithName("Update.exe").ToString(),
                Arguments =
                    new[] {
                        "--processStart=" + Assembly.GetEntryAssembly().Location.ToAbsoluteFilePath().FileName,
                        "--process-start-args=" + args.CombineParameters()
                    }.CombineParameters()
            };
        }
    }
}