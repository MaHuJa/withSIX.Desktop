// <copyright company="SIX Networks GmbH" file="GameAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Core.Games.Attributes
{
    public class GameAttribute : Attribute
    {
        public GameAttribute(string id) {
            Id = new Guid(id);
            Image = Tools.Transfer.JoinUri(CommonUrls.ImageCdn, "games", Id + "-" + "40x40" + ".png");
            BackgroundImage = Tools.Transfer.JoinUri(CommonUrls.ImageCdn, "games", Id + "-" + "full" + ".png");
            Dlcs = new string[0];
        }

        public Guid Id { get; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string FirstTimeRunInfo { get; set; }
        public string[] Executables { get; set; }
        public string[] MultiplayerExecutables { get; set; }
        public string[] ServerExecutables { get; set; }
        public LaunchType[] LaunchTypes { get; set; } = {LaunchType.Default};
        public Uri Image { get; set; }
        public Uri BackgroundImage { get; set; }
        public string[] Dlcs { get; set; }
    }
}