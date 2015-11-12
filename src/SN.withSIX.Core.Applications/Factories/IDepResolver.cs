// <copyright company="SIX Networks GmbH" file="IDepResolver.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;

namespace SN.withSIX.Core.Applications.Factories
{
    public interface IDepResolver
    {
        T GetInstance<T>() where T : class;
        IEnumerable<T> GetInstances<T>() where T : class;
    }
}