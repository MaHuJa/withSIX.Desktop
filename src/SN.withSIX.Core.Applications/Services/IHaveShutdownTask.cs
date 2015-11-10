// <copyright company="SIX Networks GmbH" file="IHaveShutdownTask.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Caliburn.Micro;

namespace SN.withSIX.Core.Applications.Services
{
    public interface IHaveShutdownTask
    {
        IResult GetShutdownTask();
    }
}