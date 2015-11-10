// <copyright company="SIX Networks GmbH" file="ActualTypeAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.ContentEngine.Infra
{
    public class ActualTypeAttribute : Attribute
    {
        public ActualTypeAttribute(params Type[] types) {
            Types = types;
        }

        public Type[] Types { get; set; }
    }
}