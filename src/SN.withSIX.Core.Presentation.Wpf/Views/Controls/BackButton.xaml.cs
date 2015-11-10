// <copyright company="SIX Networks GmbH" file="BackButton.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class BackButton : UserControl
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof (ICommand), typeof (BackButton), new PropertyMetadata(default(ICommand)));
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText",
            typeof (string), typeof (BackButton), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SubHeaderTextProperty = DependencyProperty.Register("SubHeaderText",
            typeof (string), typeof (BackButton), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register("BackCommand",
            typeof (ICommand), typeof (BackButton), new PropertyMetadata(default(ICommand)));

        public BackButton() {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public string HeaderText
        {
            get { return (string) GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public string SubHeaderText
        {
            get { return (string) GetValue(SubHeaderTextProperty); }
            set { SetValue(SubHeaderTextProperty, value); }
        }
        public ICommand BackCommand
        {
            get { return (ICommand) GetValue(BackCommandProperty); }
            set { SetValue(BackCommandProperty, value); }
        }
    }
}