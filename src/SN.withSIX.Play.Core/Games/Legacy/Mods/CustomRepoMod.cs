// <copyright company="SIX Networks GmbH" file="CustomRepoMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DoNotObfuscate]
    public class CustomRepoMod : Mod
    {
        public CustomRepoMod(Guid id) : base(id) {}
        public override bool IsCustomContent
        {
            get { return true; }
        }

        public override string GetRemotePath() {
            return Name;
        }

        protected override string GetSlugType() {
            return typeof (Mod).Name.ToUnderscore() + "s";
        }
    }
}