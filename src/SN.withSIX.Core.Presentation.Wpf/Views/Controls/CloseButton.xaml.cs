// <copyright company="SIX Networks GmbH" file="CloseButton.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class CloseButton : UserControl
    {
        public static readonly DependencyProperty SwitchCommandProperty =
            DependencyProperty.Register("SwitchCommand", typeof (ICommand), typeof (CloseButton),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof (string), typeof (CloseButton),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SmallHeaderProperty =
            DependencyProperty.Register("SmallHeader", typeof (bool), typeof (CloseButton),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IsCancelProperty = DependencyProperty.Register("IsCancel",
            typeof (bool), typeof (CloseButton), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty IsDefaultProperty = DependencyProperty.Register("IsDefault",
            typeof (bool), typeof (CloseButton), new PropertyMetadata(default(bool)));

        public CloseButton() {
            InitializeComponent();
        }

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public ICommand SwitchCommand
        {
            get { return (ICommand) GetValue(SwitchCommandProperty); }
            set { SetValue(SwitchCommandProperty, value); }
        }
        public bool SmallHeader
        {
            get { return (bool) GetValue(SmallHeaderProperty); }
            set { SetValue(SmallHeaderProperty, value); }
        }
        public bool IsCancel
        {
            get { return (bool) GetValue(IsCancelProperty); }
            set { SetValue(IsCancelProperty, value); }
        }
        public bool IsDefault
        {
            get { return (bool) GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }
    }
}