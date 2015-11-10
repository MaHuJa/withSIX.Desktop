// <copyright company="SIX Networks GmbH" file="SixDialogExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.Extensions
{
    public static class SixDialogExtensions
    {
        public static bool IsYes(this SixMessageBoxResult result) {
            return result == SixMessageBoxResult.Yes || result == SixMessageBoxResult.YesRemember;
        }

        public static bool IsNo(this SixMessageBoxResult result) {
            return result == SixMessageBoxResult.No || result == SixMessageBoxResult.NoRemember;
        }
    }
}