// <copyright company="SIX Networks GmbH" file="ContentExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Extensions
{
    public static class ContentExtensions
    {
        public static Dictionary<Guid, ContentState> GetStates(this IEnumerable<LocalContent> localContent) {
            return localContent.Where(x => x.ContentId != Guid.Empty)
                .DistinctBy(x => x.ContentId)
                .ToDictionary(x => x.ContentId, x => x.MapTo<ContentState>());
        }

        public static Dictionary<Guid, ContentState> GetStates(this IEnumerable<Collection> collections)
        {
            return collections.OfType<NetworkCollection>()
                .ToDictionary(x => x.Id, x => x.MapTo<ContentState>());
        }

        public static LaunchAction ToLaunchAction(this PlayAction action) {
            return action == PlayAction.Launch ? LaunchAction.Launch : LaunchAction.Default;
        }
    }
}