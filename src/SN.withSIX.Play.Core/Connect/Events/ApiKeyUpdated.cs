// <copyright company="SIX Networks GmbH" file="ApiKeyUpdated.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public class ApiKeyUpdated : EventArgs
    {
        public readonly string ApiKey;

        public ApiKeyUpdated(string apiKey) {
            ApiKey = apiKey;
        }
    }
}