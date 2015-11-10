// <copyright company="SIX Networks GmbH" file="SteamInfoAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Mini.Core.Games.Attributes
{
    public class SteamInfoAttribute : Attribute
    {
        public static readonly SteamInfoAttribute Default = new NullSteamInfo();
        protected SteamInfoAttribute() {}

        public SteamInfoAttribute(int appId) {
            Contract.Requires<ArgumentOutOfRangeException>(appId > 0);

            AppId = appId;
        }

        [Obsolete("We now only require the appId to get the game folder, use the appId only constructor.")]
        public SteamInfoAttribute(int appId, string folder) {
            Contract.Requires<ArgumentOutOfRangeException>(appId > 0);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(folder));

            AppId = appId;
            Folder = folder;
        }

        public virtual int AppId { get; }
        [Obsolete("We now only require the appId to get the game folder")]
        public virtual string Folder { get; }
        public bool DRM { get; set; }

        class NullSteamInfo : SteamInfoAttribute
        {
            public override int AppId => -1;
        }
    }
}