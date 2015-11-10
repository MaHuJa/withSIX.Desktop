// <copyright company="SIX Networks GmbH" file="DialogManagerUiExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Windows;

namespace SN.withSIX.Core.Applications.Services
{
    /// <summary>
    ///     Call from a background thread to synchronously call on the UI thread.
    /// </summary>
    public static class DialogManagerUiExtensions
    {
        /*
        public static string BrowseForFolderSync(this IDialogManager dialogManager, string selectedPath = null, string title = null) {
            return Schedule(() => dialogManager.BrowseForFolder(selectedPath, title));
        }

        public static string BrowseForFileSync(this IDialogManager dialogManager, string initialDirectory = null,
            string defaultExt = null, bool checkFileExists = true) {
            return Schedule(() => dialogManager.BrowseForFile(initialDirectory, defaultExt, checkFileExists));
        }
*/

        /*        [Obsolete("Use MessageBox instead")]
        public static SixMessageBoxResult MsgLegacySync(this IDialogManager dialogManager,
            MessageBoxDialogParams dialogParams) {
            return Schedule(() => dialogManager.MessageBoxSync(dialogParams));
        }*/

        /*
        public static Tuple<string, string, bool?> UserNamePasswordDialogSync(this IDialogManager dialogManager,
            string pleaseEnterUsernameAndPassword, string location) {
            return Schedule(() => dialogManager.UserNamePasswordDialog(pleaseEnterUsernameAndPassword, location));
        }
*/

        /*        public static Tuple<SixMessageBoxResult, string> ShowEnterConfirmDialogSync(
            this IDialogManager dialogManager, string msg, string defaultInput) {
            return Schedule(() => dialogManager.ShowEnterConfirmDialog(msg, defaultInput));
        }*/

        public static bool ExceptionDialogSync(this IDialogManager dialogManager, Exception e, string message,
            string title = null) {
            return Schedule(() => dialogManager.ExceptionDialog(e, message));
        }

        public static void ShowPopupSync(this IDialogManager dialogManager, object vm,
            IDictionary<string, object> overrideSettings = null) {
            Schedule(() => dialogManager.ShowPopup(vm, overrideSettings));
        }

        /*
        public static void ShowWindowSync(this IDialogManager dialogManager, object vm,
            IDictionary<string, object> overrideSettings = null) {
            Schedule(() => dialogManager.ShowWindow(vm, overrideSettings));
        }
*/

        public static SixMessageBoxResult MessageBoxSync(this IDialogManager dialogManager,
            MessageBoxDialogParams dialogParams) {
            return Schedule(() => dialogManager.MessageBox(dialogParams));
        }

        static T Schedule<T>(Func<T> t) {
            return Application.Current.Dispatcher.Invoke(t);
        }

        static void Schedule(Action t) {
            Application.Current.Dispatcher.Invoke(t);
        }
    }
}