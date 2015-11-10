// <copyright company="SIX Networks GmbH" file="OrderAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Mini.Applications.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrderAttribute : Attribute
    {
        public OrderAttribute(int order) {
            Order = order;
        }

        public int Order { get; set; }
    }
}