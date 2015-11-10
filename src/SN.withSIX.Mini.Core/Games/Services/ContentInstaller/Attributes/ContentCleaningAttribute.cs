// <copyright company="SIX Networks GmbH" file="ContentCleaningAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using NDepend.Path;

namespace SN.withSIX.Mini.Core.Games.Services.ContentInstaller.Attributes
{
    public class ContentCleaningAttribute : Attribute
    {
        public static readonly ContentCleaningAttribute Default = new NullContentCleaning();
        protected ContentCleaningAttribute() {}

        public ContentCleaningAttribute(IReadOnlyCollection<IRelativePath> exclusions,
            IReadOnlyCollection<string> fileTypes = null) {
            Exclusions = exclusions;
            if (fileTypes != null) {
                if (!fileTypes.Any())
                    throw new ArgumentException("must has at least a filetype specified");
                FileTypes = fileTypes;
            }

            if (!Exclusions.Any()) {
                throw new InvalidOperationException(
                    "No Exclusions are specified, this would cause all existing files to be deleted?!");
            }
        }

        public ContentCleaningAttribute(IReadOnlyCollection<IRelativePath> exclusions, params string[] fileTypes)
            : this(exclusions, (IReadOnlyCollection<string>) fileTypes) {}

        public virtual IReadOnlyCollection<IRelativePath> Exclusions { get; } = new List<IRelativePath>();
        public virtual IReadOnlyCollection<string> FileTypes { get; } = new List<string> {"*.*"};
        public virtual bool ShouldClean => true;

        class NullContentCleaning : ContentCleaningAttribute
        {
            public override bool ShouldClean => false;
        }
    }
}