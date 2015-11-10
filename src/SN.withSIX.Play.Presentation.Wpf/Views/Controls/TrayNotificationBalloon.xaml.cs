// <copyright company="SIX Networks GmbH" file="TrayNotificationBalloon.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReactiveUI;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    [DoNotObfuscateType]
    public enum TrayNotificationButtons
    {
        tnbNone,
        tnbYesNo,
        tnbAcceptDeclineIgnore
    }

    public partial class TrayNotificationBalloon : UserControl, IDisposable
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (TrayNotificationBalloon),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof (string), typeof (TrayNotificationBalloon),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty CloseNotificationCommandProperty =
            DependencyProperty.Register("CloseNotificationCommand", typeof (ICommand),
                typeof (TrayNotificationBalloon),
                new FrameworkPropertyMetadata(null));
        readonly TrayIcon _trayIcon;
        ICommand[] _buttonCommands;
        Timer _closeTimer;

        public TrayNotificationBalloon(TrayIcon trayIcon) {
            InitializeComponent();
            _trayIcon = trayIcon;

            var command = ReactiveCommand.Create();
            command.Subscribe(x => _trayIcon.CloseBalloon());
            CloseNotificationCommand = command;

            _closeTimer = new Timer(5000);
            _closeTimer.Elapsed += OnCloseTimerElapsed;
            _closeTimer.Start();
        }

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public ICommand CloseNotificationCommand
        {
            get { return (ICommand) GetValue(CloseNotificationCommandProperty); }
            set { SetValue(CloseNotificationCommandProperty, value); }
        }

        public void Dispose() {
            if (_closeTimer != null) {
                _closeTimer.Dispose();
                _closeTimer = null;
            }
        }

        public void Setup(string title, string message,
            TrayNotificationButtons tnb = TrayNotificationButtons.tnbNone, ICommand[] buttonCommands = null) {
            if (!ValidateCommands(tnb, buttonCommands))
                throw new ArgumentException("Number of buttons selected does not match number of commands passed.");

            Title = title;
            Message = message;
            SetupSelectedButtons(tnb);
            _buttonCommands = buttonCommands;
        }

        public bool ValidateCommands(TrayNotificationButtons tnb, ICommand[] buttonCommands) {
            switch (tnb) {
            case (TrayNotificationButtons.tnbYesNo):
                if (buttonCommands.Length != 2)
                    return false;
                break;

            case (TrayNotificationButtons.tnbAcceptDeclineIgnore):
                if (buttonCommands.Length != 3)
                    return false;
                break;
            }

            return true;
        }

        public void SetupSelectedButtons(TrayNotificationButtons tnb) {
            switch (tnb) {
            case (TrayNotificationButtons.tnbNone): {
                Button_0.Visibility = Visibility.Collapsed;
                Button_1.Visibility = Visibility.Collapsed;
                Button_2.Visibility = Visibility.Collapsed;
                break;
            }
            case (TrayNotificationButtons.tnbYesNo): {
                Button_0.Content = "Yes";
                Button_1.Content = "No";
                Button_0.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                Button_1.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                SetMargin(Button_0, 48, 109);
                SetMargin(Button_1, 126, 109);
                Button_2.Visibility = Visibility.Collapsed;
                break;
            }
            case (TrayNotificationButtons.tnbAcceptDeclineIgnore): {
                Button_0.Content = "Accept";
                Button_1.Content = "Decline";
                Button_2.Content = "Ignore";
                Button_0.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                Button_1.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                Button_2.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                SetMargin(Button_0, 8, 109);
                SetMargin(Button_1, 86, 109);
                SetMargin(Button_2, 164, 109);
                break;
            }
            }
        }

        void SetMargin(Button button, int left, int top) {
            var margin = button.Margin;
            margin.Left = left;
            margin.Top = top;
            button.Margin = margin;
        }

        void ButtonClick(object sender, RoutedEventArgs e) {
            var buttonNumber = int.Parse(((Button) sender).Name.Split('_')[1]);

            CloseNotificationCommand.Execute(null);

            _buttonCommands[buttonNumber].Execute(null);
        }

        void OnMouseEnter(object sender, MouseEventArgs e) {
            if (_closeTimer != null)
                _closeTimer.Stop();
        }

        void OnMouseLeave(object sender, MouseEventArgs e) {
            if (_closeTimer != null)
                _closeTimer.Start();
        }

        void OnCloseTimerElapsed(object sender, ElapsedEventArgs e) {
            _trayIcon.CloseBalloon();
        }
    }
}