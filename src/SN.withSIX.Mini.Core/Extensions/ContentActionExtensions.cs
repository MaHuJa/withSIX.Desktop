// <copyright company="SIX Networks GmbH" file="ContentActionExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Core.Extensions
{
    public static class ContentActionExtensions
    {
        // TODO: Remove need to recreate Specs..
        public static IContentAction<IInstallableContent> ToInstall(
            this IPlayContentAction<IContent> action) {
            return new InstallContentAction(
                action.Content.Where(x => x.Content is IInstallableContent)
                    .Select(x => new InstallContentSpec((IInstallableContent) x.Content, x.Constraint))
                    .ToArray(), action.CancelToken);
        }

        public static IEnumerable<ILaunchableContent> GetLaunchables(this ILaunchContentAction<IContent> action)
            => action.Content.SelectMany(x => x.Content.GetLaunchables(x.Constraint));
    }

    public static class ContentExtensions
    {
        internal static string GetContentPath(this Content content, string type) {
            return type + "/" + content.Id.ToShortId() + "/" + content.Name.Sluggify(true);
        }
    }
}