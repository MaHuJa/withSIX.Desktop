// <copyright company="SIX Networks GmbH" file="DialogManagerTaskExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace SN.withSIX.Core.Applications.Services
{
    /// <summary>
    ///     Call from a background thread to invoke asynchronously on the UI Thread
    /// </summary>
    public static class DialogManagerTaskExtensions
    {
        public static Task<string> BrowseForFolderAsync(this IDialogManager dialogManager, string selectedPath = null,
            string title = null) {
            return Schedule(() => dialogManager.BrowseForFolder(selectedPath, title));
        }

        public static Task<string> BrowseForFileAsync(this IDialogManager dialogManager, string initialDirectory = null,
            string defaultExt = null, bool checkFileExists = true) {
            return Schedule(() => dialogManager.BrowseForFile(initialDirectory, defaultExt, checkFileExists));
        }

        public static async Task<SixMessageBoxResult> MetroMessageBoxAsync(this IDialogManager dialogManager,
            MessageBoxDialogParams dialogParams) {
            return await await Schedule(() => dialogManager.MetroMessageBox(dialogParams));
        }

        public static async Task<bool?> ShowMetroDialogAsync(this IDialogManager dialogManager,
            IMetroDialog vm) {
            return await await Schedule(() => dialogManager.ShowMetroDialog(vm));
        }

        public static Task<bool?> ShowDialogAsync(this IDialogManager dialogManager, object vm,
            IDictionary<string, object> overrideSettings = null) {
            return Schedule(() => dialogManager.ShowDialog(vm, overrideSettings));
        }

        public static async Task<Tuple<SixMessageBoxResult, string>> ShowEnterConfirmDialogAsync(
            this IDialogManager dialogManager, string msg, string defaultInput) {
            return await await Schedule(() => dialogManager.ShowEnterConfirmDialog(msg, defaultInput));
        }

        public static Task<Tuple<string, string, bool?>> UserNamePasswordDialogAsync(this IDialogManager dialogManager,
            string pleaseEnterUsernameAndPassword, string location) {
            return Schedule(() => dialogManager.UserNamePasswordDialog(pleaseEnterUsernameAndPassword, location));
        }

        public static Task<bool> ExceptionDialogAsync(this IDialogManager dialogManager, Exception e, string message,
            string title = null, object window = null) {
            return Schedule(() => dialogManager.ExceptionDialog(e, message, window: window));
        }

        public static Task ShowPopupAsync(this IDialogManager dialogManager, object vm,
            IDictionary<string, object> overrideSettings = null) {
            return Schedule(() => dialogManager.ShowPopup(vm, overrideSettings));
        }

        public static Task ShowWindowAsync(this IDialogManager dialogManager, object vm,
            IDictionary<string, object> overrideSettings = null) {
            return Schedule(() => dialogManager.ShowWindow(vm, overrideSettings));
        }

        public static Task<SixMessageBoxResult> MessageBoxAsync(this IDialogManager dialogManager,
            MessageBoxDialogParams dialogParams) {
            return Schedule(() => dialogManager.MessageBox(dialogParams));
        }

        static Task<T> Schedule<T>(Func<T> t) {
            return Application.Current.Dispatcher.InvokeAsync(t).Task;
        }

        static Task Schedule(Action t) {
            return Application.Current.Dispatcher.InvokeAsync(t).Task;
        }
    }
}