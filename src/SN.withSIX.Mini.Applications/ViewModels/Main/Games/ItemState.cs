// <copyright company="SIX Networks GmbH" file="ItemState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Games
{
    public class ContentState
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public ItemState State { get; set; }
        public double Progress { get; set; }
        public string Version { get; set; }
    }

    public class ContentStateChange : IDomainEvent
    {
        public Guid GameId { get; set; }
        public Dictionary<Guid, ContentState> States { get; set; }
    }
}