// <copyright company="SIX Networks GmbH" file="LoginState.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Connect
{
    [DoNotObfuscateType]
    public enum LoginState
    {
        LoggedOut,
        LoggedIn,
        InvalidLogin
    }
}