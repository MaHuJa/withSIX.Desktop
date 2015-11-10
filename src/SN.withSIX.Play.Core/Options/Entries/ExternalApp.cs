// <copyright company="SIX Networks GmbH" file="ExternalApp.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DoNotObfuscateType]
    public enum StartupType
    {
        Singleplayer,
        Multiplayer,
        Any,
        Disabled
    }

    [DataContract(Name = "ExternalApp",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class ExternalApp : PropertyChangedBase
    {
        string _path;

        public ExternalApp(string name, string path, string parameters, bool runAsAdmin, StartupType startupType) {
            Name = name;
            Path = path;
            Parameters = parameters;
            RunAsAdmin = runAsAdmin;
            StartupType = startupType;
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }
        [DataMember]
        public string Parameters { get; set; }
        [DataMember]
        public bool RunAsAdmin { get; set; }
        [DataMember]
        public StartupType StartupType { get; set; }

        public void Launch(IProcessManager processManager) {
            Contract.Requires<ArgumentNullException>(Path != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(Path));
            //if (!UserSettings.Current.AppOptions.UseElevatedService) {
            var startInfo = new ProcessStartInfoBuilder(Path, Parameters) {
                WorkingDirectory = System.IO.Path.GetDirectoryName(Path),
                AsAdministrator = RunAsAdmin
            }.Build();

            MainLog.Logger.Info("Launching external app: " + startInfo.Format());
            processManager.StartAndForget(startInfo);
            //} else {
            //    _wcfClient.Value.Updater_StartAndForget(Path, Parameters, System.IO.Path.GetDirectoryName(Path), RunAsAdmin);
            //}
        }

        public void LaunchWithChecks(IProcessManager processManager, bool mp) {
            if (!CheckPreRequisites(mp))
                return;

            Launch(processManager);
        }

        bool CheckPreRequisites(bool mp) {
            if (StartupType == StartupType.Disabled)
                return false;

            if (mp) {
                if (StartupType == StartupType.Multiplayer)
                    return false;
            } else {
                if (StartupType == StartupType.Singleplayer)
                    return false;
            }

            var path = Path;
            if (!Directory.Exists(path) && File.Exists(path)) {
                var fileName = System.IO.Path.GetFileName(path);
                if (!string.IsNullOrWhiteSpace(fileName)) {
                    var processes = Tools.Processes.FindProcess(fileName);
                    if (processes.Any())
                        return false;
                }
            }
            return true;
        }
    }
}