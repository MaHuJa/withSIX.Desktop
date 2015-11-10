// <copyright company="SIX Networks GmbH" file="ContactListControl.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Controls;
using System.Windows.Data;
using SN.withSIX.Play.Applications.ViewModels.Connect;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Interaction logic for ContactListControl.xaml
    /// </summary>
    public partial class ContactListControl : UserControl
    {
        ConnectViewModel _dc;

        public ContactListControl() {
            InitializeComponent();
        }

        ConnectViewModel GetDc() {
            return _dc ?? (_dc = (ConnectViewModel) DataContext);
        }

        void Contacts_OnFilter(object sender, FilterEventArgs e) {
            e.Accepted = GetDc().Filter(e.Item);
        }
    }
}