// <copyright company="SIX Networks GmbH" file="ContentId.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public abstract class ContentId : HaveId<Guid>
    {
        protected ContentId(Guid id) : base(id) {
            Contract.Requires<ArgumentNullException>(id != Guid.Empty);
        }
    }

    public class CollectionId : ContentId
    {
        public CollectionId(Guid id) : base(id) {}
    }

    public class ModId : ContentId
    {
        public ModId(Guid id) : base(id) {}
    }

    public class MissionId : ContentId
    {
        public MissionId(Guid id) : base(id) {}
    }
}