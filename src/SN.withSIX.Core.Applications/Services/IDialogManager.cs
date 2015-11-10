// <copyright company="SIX Networks GmbH" file="IDialogManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.Services
{
    [DoNotObfuscate]
    [ContractClass(typeof (DialogManagerContract))]
    /// <summary>
    /// Re-usable dialogs
    /// </summary>
    public interface IDialogManager
    {
        string BrowseForFolder(string selectedPath = null, string title = null);
        string BrowseForFile(string initialDirectory = null, string defaultExt = null, bool checkFileExists = true);
        Tuple<string, string, bool?> UserNamePasswordDialog(string pleaseEnterUsernameAndPassword, string location);
        Task<Tuple<SixMessageBoxResult, string>> ShowEnterConfirmDialog(string msg, string defaultInput);
        bool ExceptionDialog(Exception e, string message, string title = null, object window = null);
        void ShowPopup(object vm, IDictionary<string, object> overrideSettings = null);
        void ShowWindow(object vm, IDictionary<string, object> overrideSettings = null);
        SixMessageBoxResult MessageBox(MessageBoxDialogParams dialogParams);
        Task<SixMessageBoxResult> MetroMessageBox(MessageBoxDialogParams dialogParams);
        Task<bool?> ShowMetroDialog(IMetroDialog vm);
        bool? ShowDialog(object vm, IDictionary<string, object> overrideSettings = null);
    }

    [ContractClassFor(typeof (IDialogManager))]
    public abstract class DialogManagerContract : IDialogManager
    {
        public string BrowseForFolder(string selectedPath = null, string title = null) {
            return default(string);
        }

        public string BrowseForFile(string initialDirectory = null, string defaultExt = null,
            bool checkFileExists = true) {
            return default(string);
        }

        /*
        public SixMessageBoxResult MessageBoxSync(MessageBoxDialogParams dialogParams) {
            Contract.Requires<ArgumentNullException>(dialogParams != null);
            return default(SixMessageBoxResult);
        }
*/

        public SixMessageBoxResult MessageBox(MessageBoxDialogParams dialogParams) {
            Contract.Requires<ArgumentNullException>(dialogParams != null);
            return default(SixMessageBoxResult);
        }

        public Task<SixMessageBoxResult> MetroMessageBox(MessageBoxDialogParams dialogParams) {
            Contract.Requires<ArgumentNullException>(dialogParams != null);
            return default(Task<SixMessageBoxResult>);
        }

        public Task<bool?> ShowMetroDialog(IMetroDialog vm) {
            Contract.Requires<ArgumentNullException>(vm != null);
            return default(Task<bool?>);
        }

        public bool? ShowDialog(object vm, IDictionary<string, object> overrideSettings = null) {
            Contract.Requires<ArgumentNullException>(vm != null);
            return default(bool?);
        }

        public Tuple<string, string, bool?> UserNamePasswordDialog(string pleaseEnterUsernameAndPassword,
            string location) {
            return default(Tuple<string, string, bool?>);
        }

        public Task<Tuple<SixMessageBoxResult, string>> ShowEnterConfirmDialog(string msg, string defaultInput) {
            return default(Task<Tuple<SixMessageBoxResult, string>>);
        }

        public bool ExceptionDialog(Exception e, string message, string title = null, object window = null) {
            Contract.Requires<ArgumentNullException>(e != null);
            return default(bool);
        }

        public void ShowPopup(object vm, IDictionary<string, object> overrideSettings = null) {
            Contract.Requires<ArgumentNullException>(vm != null);
        }

        public void ShowWindow(object vm, IDictionary<string, object> overrideSettings = null) {
            Contract.Requires<ArgumentNullException>(vm != null);
        }
    }
}