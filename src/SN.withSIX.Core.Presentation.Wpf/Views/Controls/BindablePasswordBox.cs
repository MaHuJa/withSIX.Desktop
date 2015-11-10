// <copyright company="SIX Networks GmbH" file="BindablePasswordBox.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class BindablePasswordBox : Decorator
    {
        public static readonly DependencyProperty PasswordProperty;
        readonly RoutedEventHandler savedCallback;
        bool isPreventCallback;

        static BindablePasswordBox() {
            PasswordProperty = DependencyProperty.Register(
                "Password",
                typeof (string),
                typeof (BindablePasswordBox),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPasswordPropertyChanged)
                );
        }

        public BindablePasswordBox() {
            savedCallback = HandlePasswordChanged;

            var passwordBox = new PasswordBox();
            passwordBox.PasswordChanged += savedCallback;
            Child = passwordBox;
        }

        public string Password
        {
            get { return GetValue(PasswordProperty) as string; }
            set { SetValue(PasswordProperty, value); }
        }

        static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs) {
            var bindablePasswordBox = (BindablePasswordBox) d;
            var passwordBox = (PasswordBox) bindablePasswordBox.Child;

            if (bindablePasswordBox.isPreventCallback)
                return;

            passwordBox.PasswordChanged -= bindablePasswordBox.savedCallback;
            passwordBox.Password = (eventArgs.NewValue != null) ? eventArgs.NewValue.ToString() : "";
            passwordBox.PasswordChanged += bindablePasswordBox.savedCallback;
        }

        void HandlePasswordChanged(object sender, RoutedEventArgs eventArgs) {
            var passwordBox = (PasswordBox) sender;

            isPreventCallback = true;
            Password = passwordBox.Password;
            isPreventCallback = false;
        }
    }
}