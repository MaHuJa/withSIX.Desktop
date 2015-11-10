// <copyright company="SIX Networks GmbH" file="UrlHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace SN.withSIX.Core.Helpers
{
    public static class URLHelper
    {
        public static string Sluggify(this string title) {
            return title.Sluggify(true);
        }

        public static string Sluggify(this string title, bool utf8, int maxlen = 80, bool toLower = false) {
            Contract.Requires<ArgumentNullException>(title != null);
            //if (title == null) return "";

            if (!utf8)
                title = title.Normalize(NormalizationForm.FormKD);

            var len = title.Length;
            var prevDash = false;
            var sb = new StringBuilder(len);

            for (var i = 0; i < len; i++) {
                var c = title[i];
                if (IsLowercaseAlpha(c) || IsNumeric(c)) {
                    if (prevDash) {
                        sb.Append('-');
                        prevDash = false;
                    }
                    sb.Append(c);
                } else if (IsUppercaseAlpha(c)) {
                    if (prevDash) {
                        sb.Append('-');
                        prevDash = false;
                    }
                    // tricky way to convert to lowercase
                    if (toLower)
                        sb.Append((char) (c | 32));
                    else
                        sb.Append(c);
                } else if (IsReplaceable(c)) {
                    if (!prevDash && sb.Length > 0)
                        prevDash = true;
                } else if (IsUndesired(c)) {
                    // We don't want it...
                } else if (c == '+')
                    sb.Append("plus");
                else {
                    if (utf8) {
                        if (prevDash) {
                            sb.Append('-');
                            prevDash = false;
                        }
                        sb.Append(c);
                        //sb.Append(HttpUtility.UrlEncode(c.ToString(CultureInfo.InvariantCulture), Encoding.UTF8));
                        // (HttpUtility.UrlEncode(c.ToString(CultureInfo.InvariantCulture), Encoding.UTF8)
                    } else {
                        var swap = ConvertEdgeCases(c, toLower);

                        if (swap != null) {
                            if (prevDash) {
                                sb.Append('-');
                                prevDash = false;
                            }
                            sb.Append(swap);
                        }
                    }
                }

                if (sb.Length == maxlen)
                    break;
            }

            return sb.ToString();
        }

        static bool IsUppercaseAlpha(char c) {
            return c >= 'A' && c <= 'Z';
        }

        static bool IsNumeric(char c) {
            return (c >= '0' && c <= '9');
        }

        static bool IsLowercaseAlpha(char c) {
            return (c >= 'a' && c <= 'z');
        }

        static bool IsUndesired(char c) {
            return c == '<' || c == '>' || c == '*' || c == '%' || c == '&' || c == ':' || c == '?' || c == '"' ||
                   c == '#';
        }

        static bool IsReplaceable(char c) {
            return c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=';
        }

        static string ConvertEdgeCases(char c, bool toLower = false) {
            string swap = null;
            switch (c) {
            case 'ı':
                swap = "i";
                break;
            case 'ł':
                swap = "l";
                break;
            case 'Ł':
                swap = toLower ? "l" : "L";
                break;
            case 'đ':
                swap = "d";
                break;
            case 'ß':
                swap = "ss";
                break;
            case 'ø':
                swap = "o";
                break;
            case 'Þ':
                swap = "th";
                break;
            }
            return swap;
        }
    }
}