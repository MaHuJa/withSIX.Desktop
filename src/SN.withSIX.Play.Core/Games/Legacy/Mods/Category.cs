// <copyright company="SIX Networks GmbH" file="Category.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    [DoNotObfuscate]
    public class Category : Content
    {
        public Category(Guid id) : base(id) {}
        public override bool HasNotes
        {
            get { return false; }
        }
        public override string Notes { get; set; }
        public override bool IsFavorite { get; set; }

        public override string ToString() {
            return Name ?? String.Empty;
        }
    }
}