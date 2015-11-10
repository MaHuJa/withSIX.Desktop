// <copyright company="SIX Networks GmbH" file="IHaveGuidId.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Connect
{
    public interface IHaveGuidId : IHaveId<Guid>, IComparePK<IHaveGuidId> {}
}