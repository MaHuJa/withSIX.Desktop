// <copyright company="SIX Networks GmbH" file="YamlTools.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SmartAssembly.Attributes;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Core
{
    public partial class Tools
    {
        public class YamlTools
        {
            public virtual void PrintMapping(YamlMappingNode mapping) {
                Contract.Requires<ArgumentNullException>(mapping != null);

                foreach (var entry in mapping.Children) {
                    var key = ((YamlScalarNode) entry.Key).Value;
                    var value = String.Empty;

                    try {
                        value = ((YamlScalarNode) entry.Value).Value;
                    } catch (Exception) {}

                    Console.WriteLine("{0}: {1}", key, value);
                }
            }
        }
    }

    [DoNotObfuscate]
    public class YamlExpectedOtherNodeTypeException : Exception
    {
        public YamlExpectedOtherNodeTypeException(string message) : base(message) {}
    }

    [DoNotObfuscate]
    public class YamlParseException : Exception
    {
        public YamlParseException(string message) : base(message) {}
    }
}