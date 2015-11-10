// <copyright company="SIX Networks GmbH" file="ContentsBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public abstract class ContentsBase : GameContentBase
    {
        protected ContentsBase(Guid gameId, List<ContentGuidSpec> contents) : base(gameId) {
            Contents = contents;
        }

        public List<ContentGuidSpec> Contents { get; }
        public string Name { get; set; }
    }
}