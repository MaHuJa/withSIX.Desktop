// <copyright company="SIX Networks GmbH" file="IdBased.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public abstract class IdBased : GameContentBase, IHaveId<Guid>
    {
        protected IdBased(Guid gameId, Guid id) : base(gameId) {
            Id = id;
        }

        public Guid Id { get; }
    }
}