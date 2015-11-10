// <copyright company="SIX Networks GmbH" file="Content.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public abstract class Content : IHaveId<Guid>
    {
        protected Content(Guid id) {
            Id = id;
        }

        public ContentInstalledState InstalledState { get; protected set; }
        public Guid Id { get; private set; }
    }
}