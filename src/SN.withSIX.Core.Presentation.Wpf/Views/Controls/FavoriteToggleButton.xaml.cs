// <copyright company="SIX Networks GmbH" file="FavoriteToggleButton.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class FavoriteToggleButton : UserControl
    {
        public static readonly DependencyProperty IsFavoriteProperty =
            DependencyProperty.Register("IsFavorite", typeof (bool), typeof (FavoriteToggleButton),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof (ICommand), typeof (FavoriteToggleButton), new PropertyMetadata(default(ICommand)));

        public FavoriteToggleButton() {
            InitializeComponent();
        }

        public bool IsFavorite
        {
            get { return (bool) GetValue(IsFavoriteProperty); }
            set { SetValue(IsFavoriteProperty, value); }
        }
        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            var command = Command;
            if (command != null && command.CanExecute(null))
                Command.Execute(null);
        }
    }
}