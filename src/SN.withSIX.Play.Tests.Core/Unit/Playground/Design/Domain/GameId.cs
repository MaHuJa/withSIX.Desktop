// <copyright company="SIX Networks GmbH" file="GameId.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain
{
    public class GameId : HaveId<Guid>
    {
        public GameId(Guid id) : base(id) {
            Contract.Requires<ArgumentNullException>(id != Guid.Empty);
        }
    }
}