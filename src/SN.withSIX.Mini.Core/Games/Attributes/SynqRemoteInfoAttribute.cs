// <copyright company="SIX Networks GmbH" file="SynqRemoteInfoAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Core.Games.Attributes
{
    // TODO: Convert to instance instead of static stuff..

    public abstract class RemoteInfoAttribute : Attribute
    {
        public abstract IReadOnlyCollection<KeyValuePair<Guid, Uri[]>> DefaultRemotes { get; }
        public abstract IReadOnlyCollection<KeyValuePair<Guid, Uri[]>> PremiumRemotes { get; }
    }

    public class SynqRemoteInfoAttribute : RemoteInfoAttribute
    {
        static readonly string[] premiumMirrors = Common.PremiumMirrors.Select(x => x + "/synq").ToArray();
        static readonly string[] defaultMirrors = Common.DefaultMirrors.Select(x => x + "/synq").ToArray();

        public SynqRemoteInfoAttribute(params string[] id) {
            DefaultRemotes = id.Select(x => GetSet(Guid.Parse(x))).ToArray();
            PremiumRemotes = id.Select(x => GetPremiumSet(Guid.Parse(x))).ToArray();
        }

        public override IReadOnlyCollection<KeyValuePair<Guid, Uri[]>> DefaultRemotes { get; }
        public override IReadOnlyCollection<KeyValuePair<Guid, Uri[]>> PremiumRemotes { get; }

        protected static KeyValuePair<Guid, Uri[]> GetSet(Guid repo) {
            return new KeyValuePair<Guid, Uri[]>(repo,
                defaultMirrors.Select(x => Tools.Transfer.JoinUri(new Uri(x), repo)).ToArray());
        }

        protected static KeyValuePair<Guid, Uri[]> GetPremiumSet(Guid repo) {
            return new KeyValuePair<Guid, Uri[]>(repo,
                premiumMirrors.Concat(defaultMirrors)
                    .Select(x => Tools.Transfer.JoinUri(new Uri(x), repo + "/p"))
                    .ToArray());
        }
    }
}