// <copyright company="SIX Networks GmbH" file="SingleCntentBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public abstract class SingleCntentBase : GameContentBase, IHaveId<Guid>
    {
        protected SingleCntentBase(Guid gameId, ContentGuidSpec content) : base(gameId) {
            Content = content;
        }

        public ContentGuidSpec Content { get; }
        public Guid Id => Content.Id;
    }
}