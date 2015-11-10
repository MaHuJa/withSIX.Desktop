// <copyright company="SIX Networks GmbH" file="EntryPoint.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reflection;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Updater.Presentation.Wpf
{
    public class EntryPoint
    {
        const string AppName = "SIX Updater";

        [STAThread]
        public static void Main() {
            try {
                Initialize();
            } catch (Exception ex) {
                TryLogException(ex);
                throw;
            }
        }

        static void Initialize() {
            CommonBase.AssemblyLoader = new AssemblyLoader(Assembly.GetEntryAssembly());
            StartupSequence.PreInit(AppName);
            UpdaterApp.Launch();
            if (Common.OnExit != null)
                Common.OnExit();
        }

        static void TryLogException(Exception ex) {
            try {
                MainLog.Logger.FormattedErrorException(ex, "Abnormal termination");
            } catch {}
        }
    }
}