// <copyright company="SIX Networks GmbH" file="TaskExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Extensions
{
    public static class TaskExtensions
    {
        public static Uri ProfileUrl(this CollectionModel collection, Game game) {
            return Tools.Transfer.JoinUri(game.GetUri(), "collections", new ShortGuid(collection.Id),
                collection.Slug);
        }
    }
}