// <copyright company="SIX Networks GmbH" file="IHaveModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Helpers
{
    public interface IHaveModel<T>
    {
        T Model { get; }
    }
}