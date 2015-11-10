// <copyright company="SIX Networks GmbH" file="IMapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Applications.Services
{
    public interface IMapper
    {
        TOut Map<TOut>(object input);
        TOut Map<TIn, TOut>(TIn input, TOut output);
    }
}