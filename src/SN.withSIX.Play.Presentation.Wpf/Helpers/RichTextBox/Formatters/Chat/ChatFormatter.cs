// <copyright company="SIX Networks GmbH" file="ChatFormatter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using SN.withSIX.Core.Logging;
using Xceed.Wpf.Toolkit;

namespace SN.withSIX.Play.Presentation.Wpf.Helpers.RichTextBox.Formatters.Chat
{
    public class ChatFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document) {
            return null;
        }

        public void SetText(FlowDocument document, string text) {
            var newText = ChatToXamlConverter.Convert(text);

            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(newText)))
                tr.Load(ms, DataFormats.Xaml);
        }
    }

    public class ChatFormatterSafe : ChatFormatter
    {
        public new string GetText(FlowDocument document) {
            return string.Empty;
        }

        public new void SetText(FlowDocument document, string text) {
            try {
                base.SetText(document, text);
            } catch (Exception e) {
                MainLog.Logger.FormattedWarnException(e);
            }
        }
    }
}