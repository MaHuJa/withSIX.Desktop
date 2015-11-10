// <copyright company="SIX Networks GmbH" file="ConsoleAppBootstrapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Presentation
{
    public abstract class ConsoleAppBootstrapper : AppBootstrapperBase
    {
        protected ConsoleAppBootstrapper() : base(false) {}

        protected override void ConfigureContainer() {
            base.ConfigureContainer();
            Container.Register<IShutdownHandler, ShutdownHandler>();
            Container.Register<IFirstTimeLicense, ConsoleFirstTimeLicense>();
            Container.Register<IDialogManager, ConsoleDialogManager>();
        }

        public virtual int OnStartup(params string[] args) {
            SetupContainer();
            PreStart();
            return 0;
        }

        protected abstract void PreStart();
    }

    public class ConsoleDialogManager : IDialogManager
    {
        public string BrowseForFolder(string selectedPath = null, string title = null) {
            throw new NotImplementedException();
        }

        public string BrowseForFile(string initialDirectory = null, string defaultExt = null,
            bool checkFileExists = true) {
            throw new NotImplementedException();
        }

        public Tuple<string, string, bool?> UserNamePasswordDialog(string pleaseEnterUsernameAndPassword,
            string location) {
            throw new NotImplementedException();
        }

        public Task<Tuple<SixMessageBoxResult, string>> ShowEnterConfirmDialog(string msg, string defaultInput) {
            throw new NotImplementedException();
        }

        public bool ExceptionDialog(Exception e, string message, string title = null, object window = null) {
            throw new NotImplementedException();
        }

        public void ShowPopup(object vm, IDictionary<string, object> overrideSettings = null) {
            throw new NotImplementedException();
        }

        public void ShowWindow(object vm, IDictionary<string, object> overrideSettings = null) {
            throw new NotImplementedException();
        }

        public SixMessageBoxResult MessageBox(MessageBoxDialogParams dialogParams) {
            throw new NotImplementedException();
        }

        public Task<SixMessageBoxResult> MetroMessageBox(MessageBoxDialogParams dialogParams) {
            throw new NotImplementedException();
        }

        public Task<bool?> ShowMetroDialog(IMetroDialog vm) {
            throw new NotImplementedException();
        }

        public bool? ShowDialog(object vm, IDictionary<string, object> overrideSettings = null) {
            throw new NotImplementedException();
        }
    }

    public abstract class ConsoleAppBootstrapper<T> : ConsoleAppBootstrapper where T : class, IConsoleLauncher
    {
        public override int OnStartup(params string[] args) {
            base.OnStartup(args);
            return Container.GetInstance<T>().Run(args);
        }
    }

    public interface IConsoleLauncher
    {
        int Run(params string[] args);
    }
}