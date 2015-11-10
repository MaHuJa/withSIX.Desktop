// <copyright company="SIX Networks GmbH" file="GameMetaData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public abstract class MetaData
    {
        public string Name { get; internal set; }
        public string Author { get; internal set; }
        public string Description { get; internal set; }
        public DateTime ReleasedOn { get; internal set; }
        public string Slug { get; internal set; }
        public Uri StoreUrl { get; internal set; }
        public Uri SupportUrl { get; internal set; }
        public bool IsFree { get; internal set; }
    }

    public class GameMetaData : MetaData
    {
        public string ShortName { get; set; }
    }

    public class DlcMetaData : MetaData
    {
        public string FullName { get; set; }
    }
}