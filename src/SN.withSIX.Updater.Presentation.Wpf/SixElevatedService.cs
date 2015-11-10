// <copyright company="SIX Networks GmbH" file="SixElevatedService.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using ManyConsole;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Core.Presentation.Services;
using SN.withSIX.Updater.Presentation.Wpf.Commands;

namespace SN.withSIX.Updater.Presentation.Wpf
{
    [DoNotObfuscateType]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class SixElevatedService : ServiceBase, IUpdaterWCF, IConsoleLauncher
    {
        readonly IEnumerable<BaseCommand> _commands;
        ServiceHost _host;
        Thread _workerThread;

        public SixElevatedService(IEnumerable<BaseCommand> commands) {
            _commands = commands.OrderBy(x => x.GetType().Name);
            ServiceName = SixElevatedServiceMisc.SixElevatedServiceName;
            CanShutdown = true;
            CanStop = true;
        }

        public int Run(params string[] args) {
            MainLog.Logger.Info("Args: {0}", args.CombineParameters());

            if (Environment.UserInteractive || args.Length != 0 || !SixElevatedServiceMisc.IsElevatedServiceInstalled())
                return RunCommandsAndLog(args);

            Run(this);
            return 0;
        }

        int RunCommandsAndLog(string[] args) {
            var r = RunCommands(args);
            if (r != 0)
                MainLog.Logger.Error("Error {0} dispatching command.", r);
            return r;
        }

        protected override void OnStart(params string[] args) {
            _workerThread = new Thread(WorkerThreadMain);
            _workerThread.Start();
        }

        void WorkerThreadMain() {
            _host = new ServiceHost(this, new Uri("net.pipe://localhost"));
            _host.AddServiceEndpoint(typeof (IUpdaterWCF), new NetNamedPipeBinding(), "UpdaterWCF_Pipe");
            _host.Open();
        }

        protected override void OnStop() {
            _workerThread.Abort();
            _workerThread.Join();
            _host.Close();
        }

        protected override void OnShutdown() {
            OnStop();
        }

        int RunCommands(string[] args) {
            return ConsoleCommandDispatcher.DispatchCommand(_commands, args, Console.Out);
        }

        #region IUpdaterWCF implementation

        public int PerformOperation(params string[] args) {
            MainLog.Logger.Info("Args: {0}", args.CombineParameters());
            try {
                return RunCommandsAndLog(args);
            } catch (Exception e) {
                throw new FaultException(e.ToString());
            }
        }

        public int LaunchGame(params string[] args) {
            try {
                var command = new LaunchGameCommand();
                var exitCode = ConsoleCommandDispatcher.DispatchCommand(command,
                    new[] {UpdaterCommands.LaunchGame, "--bypassUAC"}.Concat(args).ToArray(), Console.Out);
                if (exitCode != 0)
                    throw new Exception("Error dispatching launchgame");

                return command.ProcessID;
            } catch (Exception e) {
                throw new FaultException(e.ToString());
            }
        }

        #endregion
    }
}