// <copyright company="SIX Networks GmbH" file="PlayShellView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xaml;
using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using ReactiveUI;
using SimpleInjector;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Applications.MVVM.ViewModels.Popups;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Wpf.Helpers;
using SN.withSIX.Core.Presentation.Wpf.Services;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Applications.Views;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Options;
using SN.withSIX.Play.Presentation.Wpf.Views.Controls;
using Telerik.Windows.Controls;
using ThemeManager = MahApps.Metro.ThemeManager;
using XamlParseException = System.Windows.Markup.XamlParseException;

namespace SN.withSIX.Play.Presentation.Wpf.Views
{
    [DoNotObfuscate]
    public partial class PlayShellView : MetroWindow, IPlayShellView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IPlayShellViewModel), typeof (PlayShellView),
                new PropertyMetadata(null));
        static VirtualizingWrapPanel wp; // Workaround for SA?
        readonly IDialogManager _dialogManager;
        readonly IExceptionHandler _exceptionHandler;
        readonly INotificationCenterMessageHandler _handler;
        readonly Snow _snow;
        readonly UserSettings _userSettings;

        public PlayShellView(IEventAggregator eventBus, UserSettings settings,
            INotificationCenterMessageHandler handler,
            IDialogManager dialogManager, IExceptionHandler exceptionHandler) {
            InitializeComponent();

            Loaded += (sender, args) => DialogHelper.MainWindowLoaded = true;

            _userSettings = settings;
            _handler = handler;
            _dialogManager = dialogManager;
            _exceptionHandler = exceptionHandler;

            _snow = new Snow(ContentCanvas, eventBus);
            WorkaroundSystemMenu_Init();

            this.WhenActivated(d => {
                d(UserError.RegisterHandler<CanceledUserError>(x => CanceledHandler(x)));
                d(UserError.RegisterHandler<NotLoggedInUserError>(x => NotLoggedInDialog(x)));
                d(UserError.RegisterHandler<NotConnectedUserError>(x => NotConnectedDialog(x)));
                d(UserError.RegisterHandler<BusyUserError>(x => BusyDialog(x)));
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                d(this.OneWayBind(ViewModel, vm => vm.Overlay.ActiveItem, v => v.MainScreenFlyout.ViewModel));
                d(this.OneWayBind(ViewModel, vm => vm.SubOverlay, v => v.SubscreenFlyout.ViewModel));
                d(this.OneWayBind(ViewModel, vm => vm.StatusFlyout, v => v.StatusFlyout.ViewModel));
                d(this.WhenAnyObservable(x => x.ViewModel.ActivateWindows)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => DialogHelper.ActivateWindows(x)));
                d(_userSettings.AppOptions.WhenAnyValue(x => x.DisableEasterEggs)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(HandleEasterEgg));
                d(TryCreateTrayIcon());
            });

            ThemeManager.IsThemeChanged += CustomThemeManager.ThemeManagerOnIsThemeChanged;
        }

        internal TaskbarIcon TBI { get; private set; }
        public IPlayShellViewModel ViewModel
        {
            get { return (IPlayShellViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IPlayShellViewModel) value; }
        }

        async Task<RecoveryOptionResult> CanceledHandler(CanceledUserError canceledUserError) {
            MainLog.Logger.Info("User cancelled " + canceledUserError.ErrorMessage);
            return RecoveryOptionResult.CancelOperation;
        }

        async Task<RecoveryOptionResult> BusyDialog(BusyUserError busyUserError) {
            await Dispatcher.InvokeAsync(() => _dialogManager.BusyDialog());
            return RecoveryOptionResult.CancelOperation;
        }

        async Task<RecoveryOptionResult> NotConnectedDialog(NotConnectedUserError notConnectedUserError) {
            string action = null; // toDO;
            await _dialogManager.ShowPopupAsync(new RequirementsPopupViewModel(UiTaskHandlerExtensions.ToCommand(
                () => Common.App.PublishEvent(new RequestOpenBrowser(CommonUrls.LoginUrl)))) {
                    DisplayName = "Action '" + action + "' requires connection",
                    Message = "Please connect first to perform this action",
                    CommandTitle = "Connect"
                });
            // TODO: Connect and retry options
            return RecoveryOptionResult.CancelOperation;
        }

        async Task<RecoveryOptionResult> NotLoggedInDialog(NotLoggedInUserError notLoggedInUserError) {
            string action = null; // toDO;
            await _dialogManager.ShowPopupAsync(new RequirementsPopupViewModel(UiTaskHandlerExtensions.ToCommand(
                () => Common.App.PublishEvent(new RequestOpenBrowser(CommonUrls.LoginUrl)))) {
                    DisplayName = "Action '" + action + "' requires login",
                    Message = "Please login first to perform this action",
                    CommandTitle = "Login"
                });
            // TODO: Login and retry options
            return RecoveryOptionResult.CancelOperation;
        }

        void WorkaroundSystemMenu_Init() {
            Loaded += PlayShellView_OnLoaded;
            MouseUp += OnMouseUp;
            MouseDown += OnMouseDown;
            //AppIcon.MouseDown += IconMouseDown;
            //AppIcon.MouseUp += IconMouseUp;
        }

        void OnMouseUp(object sender, MouseButtonEventArgs e) {
            if (!(e.GetPosition(this).Y < TitlebarHeight))
                return;
            TitleBarMouseUp(sender, e);
            e.Handled = true;
        }

        void OnMouseDown(object sender, MouseButtonEventArgs e) {
            if (!(e.GetPosition(this).Y < TitlebarHeight) || e.Source == AppIcon)
                return;
            TitleBarMouseDown(sender, e);
            e.Handled = true;
        }

        public void HyperlinkClicked(object obj, EventArgs e) {
            var hl = (Hyperlink) obj;
            Tools.Generic.TryOpenUrl(hl.NavigateUri);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        CompositeDisposable TryCreateTrayIcon() {
            const string explorerErrorMessage =
                "This exception is usually due to explorer.exe not running when trying to create tray icon.\n" +
                "Try to resolve it by starting explorer.exe and then restarting the application.";
            const string explorerErrorTitle = "Tray icon creation failed.";

            try {
                return CreateTrayIcon();
            } catch (Win32Exception e) {
                _dialogManager.ExceptionDialog(e, explorerErrorMessage, explorerErrorTitle);
            } catch (XamlParseException e) {
                var inner = e.FindInnerException<Win32Exception>();
                if (inner != null)
                    _dialogManager.ExceptionDialog(inner, explorerErrorMessage, explorerErrorTitle);
            } catch (CompositionException e) {
                var found = FindException(e);
                if (found != null)
                    _dialogManager.ExceptionDialog(found, explorerErrorMessage, explorerErrorTitle);
            } catch (ActivationException e) {
                var found = e.FindInnerException<Win32Exception>();
                if (found != null)
                    _dialogManager.ExceptionDialog(found, explorerErrorMessage, explorerErrorTitle);
            }
            return new CompositeDisposable();
        }

        static Exception FindException(CompositionException exception) {
            foreach (var e in exception.Errors) {
                if (e.Exception is Win32Exception)
                    return e.Exception;

                var ce = e.Exception as CompositionException;
                if (ce != null) {
                    var found = FindException(ce);
                    if (found != null)
                        return found;
                }

                var ae = e.Exception as AggregateException;
                if (ae != null) {
                    var found = FindException(ae);
                    if (found != null)
                        return found;
                }

                if (!(e.Exception is XamlException))
                    continue;

                var inner = e.Exception.FindInnerException<Win32Exception>();
                if (inner != null)
                    return inner;
            }
            return null;
        }

        static Exception FindException(AggregateException exception) {
            foreach (var e in exception.Flatten().InnerExceptions) {
                if (e is Win32Exception)
                    return e;

                var ce = e as CompositionException;
                if (ce != null) {
                    var found = FindException(ce);
                    if (found != null)
                        return found;
                }

                if (!(e is XamlException))
                    continue;

                var inner = e.FindInnerException<Win32Exception>();
                if (inner != null)
                    return inner;
            }
            return null;
        }

        CompositeDisposable CreateTrayIcon() {
            var trayIcon = new TrayIcon(_handler.TrayNotification);
            TBI = trayIcon.TBI;
            trayIcon.Icon = "pack://application:,,,/withSIX-Play;component/app.ico";
            var list = new CompositeDisposable {
                this.OneWayBind(ViewModel, vm => vm.TrayIconDoubleclicked, v => v.TBI.DoubleClickCommand),
                //this.OneWayBind(ViewModel, vm => vm.Icon, v => v.TBI.Icon),
                this.OneWayBind(ViewModel, vm => vm.DisplayName, v => v.TBI.ToolTipText),
                this.OneWayBind(ViewModel, vm => vm.Settings.EnableTrayIcon, v => v.TBI.Visibility),
                Disposable.Create(() => VisualRoot.Children.Remove(trayIcon))
            };

            VisualRoot.Children.Add(trayIcon);
            return list;
        }

        void HandleEasterEgg(bool disableEasterEggs) {
            var dt = Tools.Generic.GetCurrentDateTime;
            if (!disableEasterEggs
                && dt.Month == 12
                && dt.Day >= 24 && dt.Day <= 27)
                _snow.Loaded();
            else
                _snow.Unloaded();
        }

        void Help(object sender, ExecutedRoutedEventArgs e) {
            BrowserHelper.TryOpenUrlIntegrated(CommonUrls.SuSupportUrl);
        }

        void PlayShellView_OnLoaded(object sender, RoutedEventArgs e) {
            WorkaroundSystemMenu_Loaded();
            HandleEasterEgg(_userSettings.AppOptions.DisableEasterEggs);
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        void WorkaroundSystemMenu_Loaded() {
            MouseUp -= TitleBarMouseUp;
            MouseDown -= TitleBarMouseDown;
        }
    }
}