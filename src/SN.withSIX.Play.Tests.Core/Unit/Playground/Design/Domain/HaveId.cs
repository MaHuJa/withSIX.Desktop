// <copyright company="SIX Networks GmbH" file="HaveId.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    // TODO: Equals/HashCode overrides
    public abstract class HaveId<TId> : IHaveId<TId>
    {
        protected HaveId(TId id) {
            Contract.Requires<ArgumentNullException>(!EqualityComparer<TId>.Default.Equals(id, default(TId)));
            Id = id;
        }

        public TId Id { get; private set; }
    }
}