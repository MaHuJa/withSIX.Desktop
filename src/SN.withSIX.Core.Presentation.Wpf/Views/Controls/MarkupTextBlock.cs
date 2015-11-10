// <copyright company="SIX Networks GmbH" file="MarkupTextBlock.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    [DoNotObfuscate]
    public class MyHyperlink : Hyperlink, IDisposable
    {
        public MyHyperlink() {
            Click += OnClicked;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            if (disposing)
                Click -= OnClicked;
        }

        void OnClicked(object sender, RoutedEventArgs args) {
            TryOpenUri();
        }

        void TryOpenUri() {
            try {
                Process.Start(NavigateUri.ToString());
            } catch (Exception) {}
        }
    }

    [ContentProperty("MarkupText")]
    [Localizability(LocalizationCategory.Text)]
    [DoNotObfuscate]
    public class MarkupTextBlock : TextBlock
    {
        public static readonly DependencyProperty MarkupTextProperty = DependencyProperty.Register(
            "MarkupText",
            typeof (string),
            typeof (MarkupTextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnTextMarkupChanged));
        static readonly string FlowDocumentPrefix =
            "<FlowDocument xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:controls='clr-namespace:SN.withSIX.Core.Presentation.SA.Views.Controls;assembly=" +
            typeof (MarkupTextBlock).Assembly.GetName().Name + "'><Paragraph><Span>";
        static readonly string FlowDocumentSuffix = "</Span></Paragraph></FlowDocument>";

        public MarkupTextBlock(string markupText) {
            MarkupText = markupText;
        }

        public MarkupTextBlock() {}
        [Localizability(LocalizationCategory.Text)]
        public string MarkupText
        {
            get { return Inlines.ToString(); }
            set { SetValue(MarkupTextProperty, value); }
        }

        static void OnTextMarkupChanged(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var markupTextBlock = dependencyObject as MarkupTextBlock;
            if (markupTextBlock != null) {
                var flowDocument = new StringBuilder();
                flowDocument.Append(FlowDocumentPrefix);
                flowDocument.Append(dependencyPropertyChangedEventArgs.NewValue);
                flowDocument.Append(FlowDocumentSuffix);

                var document = (FlowDocument) XamlReader.Parse(flowDocument.ToString());
                var paragraph = document.Blocks.FirstBlock as Paragraph;
                if (paragraph != null) {
                    var inline = paragraph.Inlines.FirstInline;
                    if (inline != null) {
                        paragraph.Inlines.Remove(inline);
                        markupTextBlock.Inlines.Clear();
                        markupTextBlock.Inlines.Add(inline);
                    }
                }
            }
        }
    }
}