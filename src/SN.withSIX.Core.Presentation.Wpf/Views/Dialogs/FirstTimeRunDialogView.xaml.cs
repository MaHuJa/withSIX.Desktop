// <copyright company="SIX Networks GmbH" file="FirstTimeRunDialogView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Infrastructure;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for FirstTimeRunDialogView.xaml
    /// </summary>
    [DoNotObfuscate]
    public partial class FirstTimeRunDialogView
    {
        readonly IResourceService _resources;

        public FirstTimeRunDialogView(IResourceService resources) {
            _resources = resources;
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e) {
            using (var stream = _resources.GetResource("" + "tos.rtf"))
                licenseRTB.Selection.Load(stream, DataFormats.Rtf);
        }
    }
}