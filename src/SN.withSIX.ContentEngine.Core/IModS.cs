// <copyright company="SIX Networks GmbH" file="IModS.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.ContentEngine.Core
{
    public interface IModS
    {
        // ReSharper disable InconsistentNaming (Javascript)
        void processMod();
        void setToken(string accessToken);
        // ReSharper restore InconsistentNaming
    }
}