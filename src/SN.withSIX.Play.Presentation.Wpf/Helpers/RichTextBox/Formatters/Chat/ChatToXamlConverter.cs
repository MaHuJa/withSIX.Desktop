// <copyright company="SIX Networks GmbH" file="ChatToXamlConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Connect.Parser;

namespace SN.withSIX.Play.Presentation.Wpf.Helpers.RichTextBox.Formatters.Chat
{
    public class ChatToXamlConverter
    {
        static readonly string xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        static readonly string cachedImageNS = Common.Flags.Merged
            ? "clr-namespace:SN.withSIX.Core.Presentation.Wpf.Views.Controls;assembly=withSIX-Play"
            : "clr-namespace:SN.withSIX.Core.Presentation.Wpf.Views.Controls;assembly=SN.withSIX.Core.Presentation.Wpf";
        static readonly string xamlHyperlink = "Hyperlink";
        static readonly string xamlHyperlinkNavigateUri = "NavigateUri";
        static readonly string xamlHyperlinkTargetName = "TargetName";
        static readonly string xamlSection = "Section";
        static readonly string xamlLineBreak = "LineBreak";
        static readonly string xamlParagraph = "Paragraph";

        public static string Convert(string ChatText) {
            var tree = new XmlDocument();
            var root = tree.CreateElement(null, xamlSection, xamlNamespace);
            var paragraph = tree.CreateElement(null, xamlParagraph, xamlNamespace);

            AddAllElements(paragraph, ChatTokenizer.Tokenize(ChatText));

            root.AppendChild(paragraph);

            root.SetAttribute("xml:space", "preserve");
            return root.OuterXml;
        }

        static void AddAllElements(XmlElement parentElement, IEnumerable<ChatTokenDto> chatTokens) {
            foreach (var ct in chatTokens) {
                switch (ct.TokenType) {
                case ChatTokenType.Text: {
                    AddText(parentElement, ct.Content);
                    break;
                }

                case ChatTokenType.LineBreak: {
                    AddLineBreak(parentElement);
                    break;
                }

                case ChatTokenType.Image: {
                    AddImage(parentElement, ct.Content);
                    break;
                }

                case ChatTokenType.Link: {
                    AddLink(parentElement, ct.Content);
                    break;
                }
                }
            }
        }

        static void AddText(XmlElement parentElement, string text) {
            Contract.Requires<ArgumentNullException>(parentElement != null);
            Contract.Requires<ArgumentNullException>(parentElement.OwnerDocument != null);
            Contract.Requires<ArgumentNullException>(text != null);

            var textElement = parentElement.OwnerDocument.CreateTextNode(text);

            parentElement.AppendChild(textElement);
        }

        static void AddLineBreak(XmlElement parentElement) {
            Contract.Requires<ArgumentNullException>(parentElement != null);
            Contract.Requires<ArgumentNullException>(parentElement.OwnerDocument != null);

            var lineBreakElement = parentElement.OwnerDocument.CreateElement(null, xamlLineBreak, xamlNamespace);

            parentElement.AppendChild(lineBreakElement);
        }

        static void AddImage(XmlElement parentElement, string url) {
            Contract.Requires<ArgumentNullException>(parentElement != null);
            Contract.Requires<ArgumentNullException>(parentElement.OwnerDocument != null);
            Contract.Requires<ArgumentNullException>(url != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(url));

            var imageElement = parentElement.OwnerDocument.CreateElement("CControls",
                "CachedImageWithAnimatedGifSupport", cachedImageNS);

            imageElement.SetAttribute("ImageUrl", url);
            imageElement.SetAttribute("RenderOptions.BitmapScalingMode", "NearestNeighbor");
            imageElement.SetAttribute("Stretch", "None");

            parentElement.AppendChild(imageElement);
        }

        static void AddLink(XmlElement parentElement, string url) {
            Contract.Requires<ArgumentNullException>(parentElement != null);
            Contract.Requires<ArgumentNullException>(parentElement.OwnerDocument != null);
            Contract.Requires<ArgumentNullException>(url != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(url));

            var linkElement = parentElement.OwnerDocument.CreateElement(null, xamlHyperlink, xamlNamespace);

            var urlParts = url.Split('#');
            if (urlParts.Length > 0 && urlParts[0].Trim().Length > 0)
                linkElement.SetAttribute(xamlHyperlinkNavigateUri, urlParts[0].Trim());
            if (urlParts.Length == 2 && urlParts[1].Trim().Length > 0)
                linkElement.SetAttribute(xamlHyperlinkTargetName, urlParts[1].Trim());

            AddText(linkElement, url);

            parentElement.AppendChild(linkElement);
        }
    }
}