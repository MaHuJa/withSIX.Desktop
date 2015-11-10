// <copyright company="SIX Networks GmbH" file="WpfDialogManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using ReactiveUI;
using SharpCompress.Compressor.Deflate;
using SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.SA;
using SN.withSIX.Core.Presentation.SA.ViewModels;
using SN.withSIX.Core.Presentation.SA.Views;
using SN.withSIX.Core.Presentation.Wpf.Helpers;
using ViewLocator = ReactiveUI.ViewLocator;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    /// <summary>
    ///     Re-usable dialogs
    /// </summary>
    // TODO: It would probably be nicer if we would not have to manually marshall these calls to the UI thread,
    // but instead (like RXUI), rely on the caller to be on the right thread already?
    // Since ViewModels are supposed to open these it could make sense.
    // The main concern with current approach is what to do when already on the UI thread... as .Wait/.Result can cause a deadlock...
    public class WpfDialogManager : IDialogManager, IEnableLogging
    {
        readonly IWindowManager _windowManager;

        public WpfDialogManager(IWindowManager windowManager) {
            _windowManager = windowManager;
            if (Application.Current != null)
                Common.App.IsWpfApp = true;
        }

        public string BrowseForFolder(string selectedPath = null, string title = null) {
            ConfirmAccess();
            var dialog = new VistaFolderBrowserDialog {SelectedPath = selectedPath, Description = title};
            return dialog.ShowDialog() == true ? dialog.SelectedPath : null;
        }

        public string BrowseForFile(string initialDirectory = null, string defaultExt = null,
            bool checkFileExists = true) {
            ConfirmAccess();
            var dialog = new OpenFileDialog {
                InitialDirectory = initialDirectory,
                DefaultExt = defaultExt,
                CheckFileExists = checkFileExists
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public Tuple<string, string, bool?> UserNamePasswordDialog(string title, string location) {
            ConfirmAccess();
            var ev = new EnterUserNamePasswordViewModel {
                DisplayName = title,
                Location = location
            };
            var result = ShowDialog(ev);
            return Tuple.Create(ev.Username, ev.Password, result);
        }

        public Task<Tuple<SixMessageBoxResult, string>> ShowEnterConfirmDialog(string msg, string defaultInput) {
            ConfirmAccess();
            var vm = new EnterConfirmViewModel {
                Message = msg,
                Input = defaultInput,
                RememberedState = false
            };

            //await ShowMetroDialog(vm);
            ShowDialog(vm);

            return
                Task.FromResult(new Tuple<SixMessageBoxResult, string>(
                    vm.Canceled
                        ? SixMessageBoxResult.Cancel
                        : (vm.RememberedState == true ? SixMessageBoxResult.YesRemember : SixMessageBoxResult.Yes),
                    vm.Input));
        }

        public bool ExceptionDialog(Exception e, string message, string title = null, object window = null) {
            ConfirmAccess();
            this.Logger().FormattedWarnException(e);
            if (Common.Flags.IgnoreErrorDialogs)
                return false;

            if (title == null)
                title = "A problem has occurred";

            var w32 = e as Win32Exception;
            if (w32 != null) {
                if (w32.NativeErrorCode == Win32ErrorCodes.FILE_NOT_FOUND) {
                    // TODO: Would be nice if we could actually add the damn file name + path!
                    message +=
                        "\n\nPossible causes:\n-A required file on the system is missing, make sure it was not accidentally deleted, or quarantined (possibly without reason) by your AntiVirus/Security suite.";
                }
            }

            var zlibEx = e as ZlibException;
            if (zlibEx != null) {
                message +=
                    "\n\nPossible causes: Corrupted downloads caused by security suite (e.g AV / FW), proxy, hardware or driver failure, or because the online files are corrupted (in that case, contact the custom repository owner or official network Support)";
            }

            var msg = KnownExceptions.ParseMessage(
                String.Format("Problem:\n{0}, {1}", e.GetType().Name, e.Message), false);
            if (!String.IsNullOrWhiteSpace(message))
                msg = String.Format("{0}<LineBreak /><LineBreak />{1}", KnownExceptions.ParseMessage(message), msg);

            return Application.Current != null && ShowExceptionDialog(msg, title, e, window);
        }

        public void ShowWindow(object vm, IDictionary<string, object> overrideSettings = null) {
            ConfirmAccess();
            var defaultSettings = new Dictionary<string, object> {
                {"ShowInTaskbar", false},
                {"WindowStyle", WindowStyle.None}
            };
            _windowManager.ShowWindow(vm, null, defaultSettings.MergeIfOverrides(overrideSettings));
        }

        public void ShowPopup(object vm, IDictionary<string, object> overrideSettings = null) {
            ConfirmAccess();
            var defaultSettings = new Dictionary<string, object> {
                {"ShowInTaskbar", false},
                {"WindowStyle", WindowStyle.None},
                {"StaysOpen", false}
            };

            _windowManager.ShowPopup(vm, null, defaultSettings.MergeIfOverrides(overrideSettings));
        }

        public SixMessageBoxResult MessageBox(MessageBoxDialogParams dialogParams) {
            ConfirmAccess();
            var ev = GetMessageBoxViewModel(dialogParams);
            ShowDialog(ev);
            var vm = ev;
            return vm.Result;
        }

        public bool? ShowDialog(object ev, IDictionary<string, object> overrideSettings = null) {
            var defaultSettings = GetDefaultDialogSettings();
            return _windowManager.ShowDialog(ev, null, defaultSettings.MergeIfOverrides(overrideSettings));
        }

        // TODO: Consider using messageboxes directly like any other View with actions?
        public async Task<SixMessageBoxResult> MetroMessageBox(MessageBoxDialogParams dialogParams) {
            ConfirmAccess();
            var ev = GetMetroMessageBoxViewModel(dialogParams);
            await ShowMetroDialog(ev);
            var vm = ev;
            return vm.Result;
        }

        /// <summary>
        ///     DO NOT CALL .RESULT or .WAIT from UI thread!
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool?> ShowMetroDialog(IMetroDialog model) {
            ConfirmAccess();
            // Da faq mahapps!!!
            var resolvedView = ViewLocator.Current.ResolveView(model);
            resolvedView.ViewModel = model;

            var view = resolvedView as BaseMetroDialog;
            if (view != null)
                return await HandleMetroDialog(model, view);

            var dialog = new CustomDialog {Content = resolvedView};
            using (model.WhenAnyValue(x => x.DisplayName).BindTo(dialog, x => x.Title))
                return await HandleMetroDialog(model, dialog);
        }

        static MessageBoxViewModel GetMessageBoxViewModel(MessageBoxDialogParams dialogParams) {
            MessageBoxViewModel ev;
            if (dialogParams.IgnoreContent) {
                ev = new MessageBoxViewModel(dialogParams.Message, dialogParams.Title,
                    GetButton(dialogParams.Buttons), dialogParams.RememberedState);
            } else {
                ev = new MessageBoxViewModel(dialogParams.Message, dialogParams.Title,
                    GetButton(dialogParams.Buttons), dialogParams.RememberedState) {
                        GreenButtonContent = dialogParams.GreenContent,
                        BlueButtonContent = dialogParams.BlueContent,
                        RedButtonContent = dialogParams.RedContent
                    };
            }
            return ev;
        }

        static MetroMessageBoxViewModel GetMetroMessageBoxViewModel(MessageBoxDialogParams dialogParams) {
            MetroMessageBoxViewModel ev;
            if (dialogParams.IgnoreContent) {
                ev = new MetroMessageBoxViewModel(dialogParams.Message, dialogParams.Title,
                    GetButton(dialogParams.Buttons), dialogParams.RememberedState);
            } else {
                ev = new MetroMessageBoxViewModel(dialogParams.Message, dialogParams.Title,
                    GetButton(dialogParams.Buttons), dialogParams.RememberedState) {
                        GreenButtonContent = dialogParams.GreenContent,
                        BlueButtonContent = dialogParams.BlueContent,
                        RedButtonContent = dialogParams.RedContent
                    };
            }
            return ev;
        }

        static void ConfirmAccess() {
            if (!Application.Current.Dispatcher.CheckAccess())
                throw new InvalidOperationException("Must be called from UI thread");
        }

        static Dictionary<string, object> GetDefaultDialogSettings() {
            return new Dictionary<string, object> {
                {"ShowInTaskbar", false},
                {"WindowStyle", WindowStyle.None}
            };
        }

        static async Task<bool?> HandleMetroDialog(IMetroDialog model, BaseMetroDialog dialog) {
            var window = (MetroWindow) Application.Current.MainWindow;
            var tcs = new TaskCompletionSource<bool?>();
            model.Close = CreateCommand(dialog, window, tcs);
            await window.ShowMetroDialogAsync(dialog);
            return await tcs.Task;
        }

        static ReactiveCommand<bool?> CreateCommand(BaseMetroDialog dialog, MetroWindow window,
            TaskCompletionSource<bool?> tcs) {
            var command = ReactiveCommand.CreateAsyncTask(async x => {
                await window.HideMetroDialogAsync(dialog);
                return (bool?) x;
            });
            SetupCommand(tcs, command);
            return command;
        }

        static void SetupCommand(TaskCompletionSource<bool?> tcs, ReactiveCommand<bool?> command) {
            var d = new CompositeDisposable {command};
            d.Add(command.ThrownExceptions.Subscribe(x => {
                tcs.SetException(x);
                d.Dispose();
            }));
            d.Add(command.Subscribe(x => {
                tcs.SetResult(x);
                d.Dispose();
            }));
        }

        static MessageBoxButton GetButton(SixMessageBoxButton button) {
            return (MessageBoxButton) Enum.Parse(typeof (MessageBoxButton), button.ToString());
        }

        static bool ShowExceptionDialog(string message, string title, Exception e, object window = null) {
            message = new XmlSanitizer().SanitizeXmlString(message);

            var ev = new ExceptionDialogViewModel(e.Format()) {
                Message = message,
                Title = title,
                Exception = e
            };

            var w = new ExceptionDialogView {DataContext = ev};
            if (window == null)
                DialogHelper.SetMainWindowOwner(w);
            else
                w.Owner = (Window) window;
            w.ShowDialog();

            if (ev.Throw)
                throw new ExceptionDialogThrownException("Redirected exception", e);

            return ev.Cancel;
        }
    }

    class ExceptionDialogThrownException : Exception
    {
        public ExceptionDialogThrownException(string message, Exception exception) : base(message, exception) {}
    }
}