// <copyright company="SIX Networks GmbH" file="SwitchButtonRight.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class SwitchButtonRight : UserControl
    {
        public static readonly DependencyProperty SwitchCommandProperty =
            DependencyProperty.Register("SwitchCommand", typeof (ICommand), typeof (SwitchButtonRight),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IsSwitchEnabledProperty =
            DependencyProperty.Register("IsSwitchEnabled", typeof (bool), typeof (SwitchButtonRight),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
            typeof (SwitchButtonRight), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ActiveItemProperty = DependencyProperty.Register("ActiveItem",
            typeof (object), typeof (SwitchButtonRight), new PropertyMetadata(default(object)));

        public SwitchButtonRight() {
            InitializeComponent();
        }

        public bool IsSwitchEnabled
        {
            get { return (bool) GetValue(IsSwitchEnabledProperty); }
            set { SetValue(IsSwitchEnabledProperty, value); }
        }
        public ICommand SwitchCommand
        {
            get { return (ICommand) GetValue(SwitchCommandProperty); }
            set { SetValue(SwitchCommandProperty, value); }
        }
        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public object ActiveItem
        {
            get { return GetValue(ActiveItemProperty); }
            set { SetValue(ActiveItemProperty, value); }
        }
    }
}