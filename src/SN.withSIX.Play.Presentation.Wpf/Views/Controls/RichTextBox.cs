// <copyright company="SIX Networks GmbH" file="RichTextBox.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    public class RichTextBox : Xceed.Wpf.Toolkit.RichTextBox
    {
        public RichTextBox() {
            TextChanged += HookHyperlinks;
        }

        void HookHyperlinks(object sender, TextChangedEventArgs e) {
            var doc = (sender as RichTextBox).Document;

            for (var position = doc.ContentStart;
                position != null && position.CompareTo(doc.ContentEnd) <= 0;
                position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) ==
                    TextPointerContext.ElementEnd) {
                    if (position.Parent is Hyperlink)
                        (position.Parent as Hyperlink).RequestNavigate += HandleRequestNavigate;
                }
            }
        }

        void HandleRequestNavigate(object sender, RequestNavigateEventArgs args) {
            if (CommonUrls.IsWithSixUrl(args.Uri))
                BrowserHelper.TryOpenUrlIntegrated(args.Uri);
            else
                Tools.Generic.TryOpenUrl(args.Uri);
        }
    }
}