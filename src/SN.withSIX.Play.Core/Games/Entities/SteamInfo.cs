// <copyright company="SIX Networks GmbH" file="SteamInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public class SteamInfo
    {
        protected SteamInfo() {}

        public SteamInfo(int appId) {
            Contract.Requires<ArgumentOutOfRangeException>(appId > 0);

            AppId = appId;
        }

        [Obsolete("We now only require the appId to get the game folder, use the appId only constructor.")]
        public SteamInfo(int appId, string folder) {
            Contract.Requires<ArgumentOutOfRangeException>(appId > 0);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(folder));

            AppId = appId;
            Folder = folder;
        }

        public virtual int AppId { get; }
        [Obsolete("We now only require the appId to get the game folder")]
        public virtual string Folder { get; }
        public bool DRM { get; internal set; }
    }

    public class NullSteamInfo : SteamInfo
    {
        public override int AppId
        {
            get { return -1; }
        }
    }
}