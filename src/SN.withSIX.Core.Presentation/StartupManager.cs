// <copyright company="SIX Networks GmbH" file="StartupManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Configuration;
using System.Threading.Tasks;
using AutoMapper.Mappers;
using Microsoft.Win32;
using NDepend.Path;
using SevenZip;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Assemblies;
using SN.withSIX.Sync.Core.Legacy.SixSync;

namespace SN.withSIX.Core.Presentation
{
    public class StartupManager : IStartupManager, IEnableLogging
    {
        static ListSourceMapper _dummyForAutoMapper = new ListSourceMapper();
        readonly ICacheManager _cacheManager;
        readonly ISystemInfo _systemInfo;

        public StartupManager(ISystemInfo systemInfo, ICacheManager cacheManager) {
            _systemInfo = systemInfo;
            _cacheManager = cacheManager;
        }

        public async Task Exit() {
            try {
                await TryExit().ConfigureAwait(false);
            } catch (Exception e) {
                MainLog.Logger.FormattedErrorException(e,
                    "Error during Application shutdown. Usually these are swallowed.");
                throw;
            }
        }

        public virtual void RegisterServices() {
            Setup7z(Common.Paths.SharedDllPath);
            Repository.InitializeConfig(
                PathConfiguration.GetRoamingRootPath().GetChildDirectoryWithName("Synq"));
        }

        public void RegisterUserAppKeys() {
            var key = Registry.CurrentUser.CreateSubKey(Common.AppCommon.ApplicationRegKey);
            key.SetValue("Path", Common.Paths.EntryLocation);
            key.SetValue("Version", CommonBase.AssemblyLoader.GetEntryVersion());
        }

        async Task TryExit() {
            await ExitI().ConfigureAwait(false);

            var tspan = Tools.Generic.GetCurrentUtcDateTime - Common.StartTime;
            var lifeTime = CalculateLifeSpan(tspan);
            MainLog.Logger.Info("Exiting application, lifetime: {0} ({1}s)", lifeTime, tspan.TotalSeconds);
            UsageCounter.ReportUsage(String.Format("Play - Uptime: {0}", lifeTime));
        }

        protected virtual async Task ExitI() {
            await _cacheManager.Vacuum().ConfigureAwait(false);
            // TODO: Or should we do this on startup, or even periodically?
            await _cacheManager.Shutdown().ConfigureAwait(false);
        }

        static string CalculateLifeSpan(TimeSpan tspan) {
            if (tspan.TotalDays >= 30)
                return "More than a month";
            if (tspan.TotalDays > 14)
                return "More than two weeks";
            if (tspan.TotalDays > 7)
                return "More than a week";
            if (tspan.TotalDays > 2)
                return "More than two days";
            if (tspan.TotalDays > 1)
                return "More than a day";
            if (tspan.TotalHours > 16)
                return "More than 16 hours";
            if (tspan.TotalHours > 8)
                return "More than 8 hours";
            if (tspan.TotalHours > 4)
                return "More than 4 hours";
            if (tspan.TotalHours > 2)
                return "More than 2 hours";
            if (tspan.TotalHours > 1)
                return "More than an hour";
            if (tspan.TotalMinutes > 30)
                return "More than 30 minutes";
            if (tspan.TotalMinutes > 10)
                return "More than 10 minutes";
            if (tspan.TotalMinutes > 5)
                return "More than 5 minutes";
            if (tspan.TotalMinutes > 1)
                return "More than a minute";
            return "Less than a minute";
        }

        public static void Setup7z(IAbsoluteDirectoryPath dllPath) {
            var path = dllPath.GetChildDirectoryWithName(AssemblyHandler.Bitness).GetChildFileWithName("7z.dll").ToString();
            ConfigurationManager.AppSettings["7zLocation"] = path;
            SevenZipBase.SetLibraryPath(path);
        }
    }
}