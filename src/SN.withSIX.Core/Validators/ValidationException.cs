// <copyright company="SIX Networks GmbH" file="ValidationException.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;

namespace SN.withSIX.Core.Validators
{
    public static class UriValidator
    {
        public static bool IsValidUri(string value, params string[] specificSchemes) {
            if (value == null)
                return true;

            if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
                return false;

            return specificSchemes == null || !specificSchemes.Any() || specificSchemes.Contains(new Uri(value).Scheme);
        }

        public static bool IsValidUriAllowOmniProtocol(string value, params string[] specificSchemes) {
            if (value == null)
                return true;

            if (value.StartsWith("//"))
                value = "http:" + value;

            return IsValidUri(value, specificSchemes);
        }

        public static bool IsValidUriWithHttpFallback(string value, params string[] specificSchemes) {
            if (value == null)
                return true;

            if (IsValidUri(value, specificSchemes))
                return true;

            value = "http://" + value;
            return IsValidUri(value, specificSchemes);
        }
    }
}