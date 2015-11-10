// <copyright company="SIX Networks GmbH" file="PackagedContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class PackagedContent : InstallableContent, IPackagedContent
    {
        protected PackagedContent() {}

        protected PackagedContent(string name, string packageName, Guid gameId) : base(name, gameId) {
            PackageName = packageName;
        }

        [DataMember]
        public string PackageName { get; set; }

        public override IEnumerable<IContentSpec<Content>> GetRelatedContent(List<IContentSpec<Content>> list = null,
            string constraint = null) {
            if (list == null)
                list = new List<IContentSpec<Content>>();

            if (list.Select(x => x.Content).Contains(this))
                return list;

            var spec = new PackagedContentSpec(this, constraint);
            list.Add(spec);
            return list;
        }
    }
}