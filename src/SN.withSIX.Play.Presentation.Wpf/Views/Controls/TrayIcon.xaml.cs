// <copyright company="SIX Networks GmbH" file="TrayIcon.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xaml;
using System.Xml;
using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Events;
using SN.withSIX.Core.Applications.MVVM;
using XamlParseException = System.Windows.Markup.XamlParseException;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    public partial class TrayIcon : UserControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof (string),
            typeof (TrayIcon), new PropertyMetadata(default(string)));
        readonly ConcurrentQueue<ITrayNotification> _notificationQueue;
        TrayNotificationBalloon _lastTrayBalloon;
        bool _trayIsReady = true;

        public TrayIcon(IObservable<ITrayNotification> notifications) {
            InitializeComponent();
            Application.Current.Exit += (sender, args) => TaskbarIcon.Dispose();
            _notificationQueue = new ConcurrentQueue<ITrayNotification>();
            // TODO: Just attach through ViewModel instead...
            notifications.Subscribe(x => {
                _notificationQueue.Enqueue(x);
                ProcessNotificationQueue();
            });
        }

        public TaskbarIcon TBI
        {
            get { return TaskbarIcon; }
        }
        public string Icon
        {
            get { return (string) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public void CloseBalloon() {
            TaskbarIcon.CloseBalloon();
            _lastTrayBalloon.Dispose();
            _trayIsReady = true;
            ProcessNotificationQueue();
        }

        void ProcessNotificationQueue() {
            ITrayNotification currentEvent;

            lock (_notificationQueue) {
                if (!_trayIsReady)
                    return;

                if (!_notificationQueue.TryDequeue(out currentEvent))
                    return;

                _trayIsReady = false;
            }

            UiHelper.TryOnUiThread(() => TryProcessNotification(currentEvent));
        }

        void TryProcessNotification(ITrayNotification currentEvent) {
            try {
                ProcessNotification(currentEvent);
            } catch (XmlException e) {
                ProcessNotificationException(e);
            } catch (XamlParseException e) {
                ProcessNotificationException(e);
            } catch (XamlException e) {
                ProcessNotificationException(e);
            }
        }

        static void ProcessNotificationException(Exception e) {
            UserError.Throw(new InformationalUserError(e,
                "A problem occurred while trying to create a popup notification", null));
        }

        void ProcessNotification(ITrayNotification currentEvent) {
            var currentNotifyEvent = currentEvent as TrayNotification;
            if (currentNotifyEvent != null) {
                ProcessBalloon(currentNotifyEvent);
                return;
            }

            var current2ChoiceEvent = currentEvent as TwoChoiceTrayNotification;
            if (current2ChoiceEvent != null) {
                Process2Choice(current2ChoiceEvent);
                return;
            }

            var current3ChoiceEvent = currentEvent as ThreeChoiceTrayNotification;
            if (current3ChoiceEvent != null)
                Process3Choice(current3ChoiceEvent);
        }

        void Process3Choice(ThreeChoiceTrayNotification current3ChoiceEvent) {
            _lastTrayBalloon = CreateBalloon(current3ChoiceEvent.Title,
                current3ChoiceEvent.Message,
                TrayNotificationButtons.tnbAcceptDeclineIgnore,
                new[] {
                    current3ChoiceEvent.AcceptCommand,
                    current3ChoiceEvent.DeclineCommand,
                    current3ChoiceEvent.IgnoreCommand
                });
            TaskbarIcon.ShowCustomBalloon(_lastTrayBalloon, PopupAnimation.Slide, null);
        }

        TrayNotificationBalloon CreateBalloon(string title, string message,
            TrayNotificationButtons tnb = TrayNotificationButtons.tnbNone, ICommand[] buttonCommands = null) {
            var balloon = new TrayNotificationBalloon(this);
            balloon.Setup(title, message, tnb, buttonCommands);
            return balloon;
        }

        void ProcessBalloon(TrayNotification currentNotifyEvent) {
            _lastTrayBalloon = CreateBalloon(currentNotifyEvent.Title,
                currentNotifyEvent.Message);
            TaskbarIcon.ShowCustomBalloon(_lastTrayBalloon, PopupAnimation.Slide, null);
        }

        void Process2Choice(TwoChoiceTrayNotification current2ChoiceEvent) {
            _lastTrayBalloon = CreateBalloon(current2ChoiceEvent.Title,
                current2ChoiceEvent.Message,
                TrayNotificationButtons.tnbYesNo,
                new[] {
                    current2ChoiceEvent.YesCommand,
                    current2ChoiceEvent.NoCommand
                });
            TaskbarIcon.ShowCustomBalloon(_lastTrayBalloon, PopupAnimation.Slide, null);
        }
    }
}