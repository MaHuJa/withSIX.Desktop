// <copyright company="SIX Networks GmbH" file="ToolsVersionAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly), DoNotObfuscateType]
    public class ToolsVersionAttribute : Attribute
    {
        public string Version;

        public ToolsVersionAttribute(string txt) {
            Version = txt;
        }
    }
}