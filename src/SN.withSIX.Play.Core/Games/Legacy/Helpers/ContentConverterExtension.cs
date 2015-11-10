// <copyright company="SIX Networks GmbH" file="ContentConverterExtension.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Games.Legacy.Helpers
{
    public static class ContentConverterExtension
    {
        public static IMod ToMod(this IContent content) {
            var togle = content as ToggleableModProxy;
            if (togle != null)
                return togle.Model;

            return content as IMod;
        }

        public static IMod ToMod(this object obj) {
            var content = obj as IContent;
            return content != null ? content.ToMod() : null;
        }
    }
}