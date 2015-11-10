// <copyright company="SIX Networks GmbH" file="PrettyPrinterExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals;
using SN.withSIX.Core.Services;

namespace SN.withSIX.Core.Extensions
{
    public static class PrettyPrinterExtensions
    {
        public static string PrettyPrintJson(this string unprettyJson) {
            var pp = new JsonPrettyPrinter(new JsonPPStrategyContext());

            return pp.PrettyPrint(unprettyJson);
        }
    }
}