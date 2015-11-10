// <copyright company="SIX Networks GmbH" file="WebApiBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;

namespace SN.withSIX.Play.Infra.Api
{
    class WebApiBase
    {
        protected static IDictionary<string, object> GetPageParam(int page) {
            return new RestParams {{"page", page}};
        }
    }
}