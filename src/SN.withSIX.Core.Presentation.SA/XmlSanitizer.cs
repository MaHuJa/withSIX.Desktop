// <copyright company="SIX Networks GmbH" file="XmlSanitizer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Text;

namespace SN.withSIX.Core.Presentation.SA
{
    public class XmlSanitizer
    {
        public string SanitizeXmlString(string xml) {
            if (xml == null)
                throw new ArgumentNullException("xml");

            var buffer = new StringBuilder(xml.Length);
            foreach (var c in xml)
                buffer.Append(IsLegalXmlChar(c) ? c : '?');

            return buffer.ToString();
        }

        bool IsLegalXmlChar(int character) {
            return
                (
                    character == 0x9 /* == '\t' == 9   */||
                    character == 0xA /* == '\n' == 10  */||
                    character == 0xD /* == '\r' == 13  */||
                    (character >= 0x20 && character <= 0xD7FF) ||
                    (character >= 0xE000 && character <= 0xFFFD) ||
                    (character >= 0x10000 && character <= 0x10FFFF)
                    );
        }
    }
}